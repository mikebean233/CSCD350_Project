using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Timers;
using Timer = System.Timers.Timer;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;

namespace MediaPlayer
{
    [DataContract]
    class MainController
    {
        private MainWindow _view;
        [DataMember]
        private MediaItem _currentItem;
        [DataMember]
        private int _volume; // 1 - 100
        [DataMember]
        private PlayModeEnum _playMode;
        [DataMember]
        private PlayStateEnum _playState;
        //[DataMember]
        public PlayModeEnum PlayMode
        {
            get { return _playMode; }
        }

        private MediaElement _mediaElement;
        private Timer _mediaElementPollingTimer;
        private DatabaseController _databaseController;
        private FileScanner _fileScanner;
        private Thread _fileScannerThread;
        private List<string> _supportedExtentions;

        private MediaLibrary _mediaLibrary;
        private bool _timerIsChangingScrubBar;
        [DataMember]
        private string _stateCaptureFileName;

        // private List<string> _selectedLibraryFiles;
         
        public List<string> SupportedExtentions { get { return _supportedExtentions; } } 

        public MainController(MainWindow view)
        {
            _view =  view;
            _supportedExtentions = new List<string>() {"*mp3", "*wma", "*wmv", "*asf"};
            _mediaLibrary = new MediaLibrary("Media Library", this);
            _stateCaptureFileName = "savedState.cfg";
            _playMode = PlayModeEnum.Consecutive; 
        }

        public void Setup()
        {
            _timerIsChangingScrubBar = false;
            _mediaElement = _view.me_MediaElement;

            _databaseController = new DatabaseController();
            _mediaLibrary.AddRange(_databaseController.GetMediaItemsFromDatabase());
            if (!_mediaLibrary.Any())
            {
                _fileScanner = new FileScanner(this);
                _fileScannerThread = new Thread(new ParameterizedThreadStart(_fileScanner.ScanDirectory));
                _fileScannerThread.Start("C:\\");
            }
            UpdateDataGrids();

            _mediaElementPollingTimer = new Timer(500); // Create a timer that polls every 1/2 second
            _mediaElementPollingTimer.Elapsed += new ElapsedEventHandler(PollingTimerHandler);
            _mediaElementPollingTimer.Start();

            /**
             *   TODO:
             *      1) load serialized state from savedState.cfg (in current directory)
             *         - If error, setup default state and create/correct savedState.cfg 
             */

            _playState = PlayStateEnum.Stopped;

        }

        public void UpdateDataGrids()
        {
          //  _view.dataGrid_MediaL.ItemsSource = _mediaLibrary.GetMedia();
            _view.Dispatcher.Invoke(new Action(() => _view.dataGrid_MediaL.ItemsSource = _mediaLibrary.GetMedia()), new object[] { });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// </returns>
        private bool loadSavedState()
        {
            // TODO: Read in state data from savedState.cfg and use it to set the state.
            return false;
        }

        public void PollingTimerHandler(object sender, ElapsedEventArgs e)
        {
            if (_playState == PlayStateEnum.Playing)
            {
                _timerIsChangingScrubBar = true;
                _view.Dispatcher.Invoke(new Action(() => CheckMediaObjectStatus()), new object[] {});
            }
        }
        
        public void CheckMediaObjectStatus()
        {
           if (_mediaElement != null && _mediaElement.IsLoaded && _mediaElement.NaturalDuration.HasTimeSpan)
           {
                // Update Time Label
                TimeSpan timeElapsed = _mediaElement.Position;
                TimeSpan totalTime = _mediaElement.NaturalDuration.TimeSpan;
               _view.lbl_ScrubBarTime.Content = Utilities.BuildStringFromTimeSpan(timeElapsed) + "/" + Utilities.BuildStringFromTimeSpan(totalTime);

                // Update Progress Slider
                double completionRatio = timeElapsed.TotalMilliseconds/totalTime.TotalMilliseconds;
                _view.slider_ScrubBar.Value = completionRatio;
            }
            _timerIsChangingScrubBar = false;
        }

        
        public void DeleteLibraryEntry() { }

        // This method is called by the FileScanner 
        // (or possibly another piece of code) when a new file needs to be added to the library
        public void AddMediaEvent(string newMediaPath)
        {
            Console.WriteLine("adding: " + newMediaPath);
            if (string.IsNullOrEmpty(newMediaPath))
                return;
            MediaItem thisItem = Utilities.BuildMediaItemFromPath(newMediaPath);
            if (thisItem != null)
            {
                _mediaLibrary.AddNewMediaItem(thisItem);
                UpdateDataGrids();
            }
            //_view.Dispatcher.Invoke(new Action(() => _databaseController.retrievePlaylistToDataGrid(_view.dataGrid_MediaL)), new object[] { });
        }

        public void FetchMediaLibraryData() { }

        #region View Events

        public void MediaEnded()
        {
            _currentItem = _mediaLibrary.GetNextSong();

            if (_currentItem != null)
                _mediaElement.Source = new Uri(_currentItem.Filepath);
        }

        public void SkipForwardButtonPressed()
        {
            Console.WriteLine("Skip Forward");
            MediaEnded();
        }

        public void SkipBackwardButtonPressed()
        {
            Console.WriteLine("Skip Backward");
        }

        public void VolumeSliderChanged(double newValue)
        {
            //Console.WriteLine("Volume Value: " + newValue);
            _view.me_MediaElement.Volume = newValue;
        }

        public void PlayButtonPressed()
        {
            //   Console.WriteLine("Play");
            _mediaElement.Play();
            _playState = PlayStateEnum.Playing;
        }
        public void StopButtonPressed()
        {
            //Console.WriteLine("Stop");
            _mediaElement.Stop();
        }

        public void PauseButtonPressed()
        {
            //Console.WriteLine("Pause");
            _mediaElement.Pause();
            _playState = PlayStateEnum.Paused;
        }

        public void ProgressBarMovedByUser(double newValue)
        {
            if (!_timerIsChangingScrubBar && _mediaElement.NaturalDuration.HasTimeSpan)
            {
                _mediaElementPollingTimer.Enabled = false;

                //    Console.WriteLine("ProgressBar Value: " + newValue);
                TimeSpan totalTime = _mediaElement.NaturalDuration.TimeSpan;
                int milliseconds   = (int)(totalTime.TotalMilliseconds * newValue) % 1000;
                int seconds        = (int)(totalTime.TotalSeconds      * newValue) % 60;
                int minutes        = (int)(totalTime.TotalMinutes      * newValue) % 60;
                int hours          = (int)(totalTime.TotalHours        * newValue) % 24;
                int days           = 0;
                _mediaElement.Position = new TimeSpan(days, hours, minutes, seconds, milliseconds ); //_mediaElement.NaturalDuration.*newValue);
                
                TimeSpan timeElapsed = _mediaElement.Position;
                _view.lbl_ScrubBarTime.Content = Utilities.BuildStringFromTimeSpan(timeElapsed) + "/" + Utilities.BuildStringFromTimeSpan(totalTime);
                _mediaElementPollingTimer.Enabled = true;
            }
        }

        public void CloseWindow()
        {
            //Console.WriteLine("Close");
            // serialize the current state of this class

            SerializeThis(this, _stateCaptureFileName);
            _databaseController.AddMediaItemsToDatabase(_mediaLibrary.GetMedia());
            if(_fileScannerThread != null && _fileScannerThread.IsAlive)
                _fileScannerThread.Abort();
            _mediaElementPollingTimer.Stop();
            _mediaElementPollingTimer.Close();
        }
        public void MediaFileError() { }

        public void DataGridRowSelected(IList items)
        {
            if (items != null && items.Count != 0)
            {
                MediaItem selectedMedia = ((MediaItem) items[0]);
                _currentItem = selectedMedia;
                _mediaElement.Source = new Uri(selectedMedia.Filepath);
                _mediaLibrary.SetCurrentMedia(selectedMedia);
                if (_playState == PlayStateEnum.Playing)
                    _mediaElement.Play();
            }
            //_selectedLibraryFiles = new List<string>();
            //foreach(System.Data.DataRowView thisRow in items)
            //    _selectedLibraryFiles.Add((string)thisRow.Row.ItemArray[1]);
        }
        
        #endregion View Events 

        public static bool SerializeThis(MainController mainController, string outputPath)
        {
            if (mainController != null && !string.IsNullOrEmpty(outputPath))
            {
                try
                {
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    //formatter.Serialize(stream, mainController);
                    //stream.Close();

                    //MemoryStream stream1 = new MemoryStream();
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(mainController.GetType() );
                    ser.WriteObject(stream, mainController);
                    stream.Close();


                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            
            }
            return false;
        }

    }
}
