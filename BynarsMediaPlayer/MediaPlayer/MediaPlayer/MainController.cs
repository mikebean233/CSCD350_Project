using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Timers;
using Timer = System.Timers.Timer;
using System.IO;
using System.Reflection;
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
        private double _volume; // [0, 1]
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

        [DataMember]
        private string _currentPlaylistName;

        // This variable is used to change the position of the media, it will be changed when the timer gets triggered
        private double _requestedPositionValue;
        // private List<string> _selectedLibraryFiles;
         
        public List<string> SupportedExtentions { get { return _supportedExtentions; } } 

        public MainController(MainWindow view)
        {
            _view =  view;
            _supportedExtentions = new List<string>() {"*mp3", "*wma", "*wmv", "*asf"};
            _mediaLibrary = new MediaLibrary("Media Library", this);
            _stateCaptureFileName = "savedState.cfg";
            _playMode = PlayModeEnum.Consecutive;
            _timerIsChangingScrubBar = false;
        }

        public bool LoadSavedState()
        {
            try
            {
                MainController lastInstance = Utilities.DeserializeObjectFromJson<MainController>(_stateCaptureFileName);
                if (lastInstance == null)
                    return false;
                _playState = lastInstance._playState;
                _playMode = lastInstance._playMode;
                _volume = lastInstance._volume;
                _currentPlaylistName = lastInstance._currentPlaylistName;

            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public void SetDefaultState()
        {
            _playState = PlayStateEnum.Stopped;
            _playMode = PlayModeEnum.Consecutive;
            _volume = 0.5;
            _currentPlaylistName = "Media Library";
        }


        public void Setup()
        {
            // Try to load the saved state, if there is a problem, set the state to default values.
            if (!LoadSavedState())
                SetDefaultState();
            
            // Get a reference to the media element object
            _mediaElement = _view.me_MediaElement;

            // Instantiate the database controller and load the entries from the database into the media library
            _databaseController = new DatabaseController();
            _mediaLibrary.AddRange(_databaseController.GetMediaItemsFromDatabase());

            // If there are no entries in the media library, kick off a thread to scan the filesystem for media, 
            // and add that to the library.
            if (!_mediaLibrary.Any())
            {
                _fileScanner = new FileScanner(this);
                _fileScannerThread = new Thread(new ParameterizedThreadStart(_fileScanner.ScanDirectory));
                _fileScannerThread.Start("C:\\");
            }

            UpdateDataGrids();
        
            // Start the polling timer (which is used to update the view)
            _mediaElementPollingTimer = new Timer(150);
            _mediaElementPollingTimer.Elapsed += new ElapsedEventHandler(PollingTimerHandler);
            _mediaElementPollingTimer.Start();
        }

        public void UpdateDataGrids()
        {
            _view.Dispatcher.Invoke(new Action(() => _view.lv_MediaLibraryView.ItemsSource = _mediaLibrary.GetMedia()), new object[] { });
        }

        private void CheckForPositionChangeRequest()
        {
            if (_requestedPositionValue != 0.0 && _mediaElement.NaturalDuration.HasTimeSpan)
            {
                _mediaElement.Position = Utilities.BuildTimspanFromPerportion(_requestedPositionValue, _mediaElement.NaturalDuration.TimeSpan);
                _requestedPositionValue = 0.0;
            }
        }
        public void PollingTimerHandler(object sender, ElapsedEventArgs e)
        {
            _view.Dispatcher.Invoke(new Action(() => CheckForPositionChangeRequest()), new object[] { });
            if (_playState == PlayStateEnum.Playing)
            {
                _timerIsChangingScrubBar = true;
                _view.Dispatcher.Invoke(new Action(() => UpdateView()), new object[] {});
                _timerIsChangingScrubBar = false;
            }
        }
        
        public void UpdateView()
        {
            if (_mediaElement != null && _mediaElement.IsLoaded && _mediaElement.NaturalDuration.HasTimeSpan && _currentItem != null)
            {
                // Update Time Label
                TimeSpan timeElapsed = _mediaElement.Position;
                TimeSpan totalTime = _mediaElement.NaturalDuration.TimeSpan;
                _view.lbl_ScrubBarTime.Content = Utilities.BuildStringFromTimeSpan(timeElapsed) + "/" +
                                                 Utilities.BuildStringFromTimeSpan(totalTime);

                // Update Progress Slider
                double completionRatio = timeElapsed.TotalMilliseconds/totalTime.TotalMilliseconds;
                _view.slider_ScrubBar.Value = completionRatio;
                _view.Dispatcher.Invoke(new Action(() => _view.lv_MediaLibraryView.ItemsSource = _mediaLibrary.GetMedia()), new object[] { });
                _currentItem.Position = completionRatio;

                // update the play mode toggle
                switch (_playMode)
                {
                    case PlayModeEnum.Consecutive:
                        _view.BTN_playMode.Content = "C";
                        break;
                    case PlayModeEnum.Repeat:
                        _view.BTN_playMode.Content = "R";
                        break;
                    case PlayModeEnum.Shuffle:
                        _view.BTN_playMode.Content = "S";
                        break;
                }
            }
            else
            {
                _view.lbl_ScrubBarTime.Content = "0:00:00/0:00:00";
                _view.slider_ScrubBar.Value = 0;
            }

            //Update the Volume Slider
            _view.slider_VolumeControl.Value = _volume;

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

        public bool ChangeCurrentMedia(MediaItem _newItem)
        {
            _mediaElementPollingTimer.Enabled = false;
            if (_newItem == null)
                return false;
            try
            {
                if (!_mediaLibrary.SetCurrentMedia(_newItem))
                    return false;
                _currentItem = _mediaLibrary.GetCurrentMedia();
                _mediaElement.Source = new Uri(_currentItem.Filepath);

                _requestedPositionValue = _currentItem.Position;

                if (_playState == PlayStateEnum.Playing)
                    _mediaElement.Play();
            }
            catch (Exception e)
            {
                return false;
            }
            this._currentItem = _mediaLibrary.GetCurrentMedia();
            _mediaElementPollingTimer.Enabled = true;
            return true;
        }


        #region View Events

        public void MediaEnded()
        {
            if (_currentItem != null)
                _currentItem.Position = 0.0;

            ChangeCurrentMedia(_mediaLibrary.GetNextSong());
        }

        public void SkipForwardButtonPressed()
        {
            Console.WriteLine("Skip Forward");
            ChangeCurrentMedia(_mediaLibrary.GetNextSong());
        }

        public void SkipBackwardButtonPressed()
        {
            ChangeCurrentMedia(_mediaLibrary.GetPreviousSong());
        }

        public void VolumeSliderChanged(double newValue)
        {
            _view.me_MediaElement.Volume = newValue;
            _volume = newValue;
        }

        public void PlayButtonPressed()
        {
            _mediaElement.Play();
            _playState = PlayStateEnum.Playing;
        }
        public void StopButtonPressed()
        {
            _mediaElement.Stop();
        }

        public void PauseButtonPressed()
        {
            _mediaElement.Pause();
            _playState = PlayStateEnum.Paused;
        }

        

        public void ProgressBarMovedByUser(double newValue)
        {
            if (!_timerIsChangingScrubBar)
                _requestedPositionValue = newValue;
        }

        public void CloseWindow()
        {
            Utilities.SerializeObjectToJson<MainController>(_stateCaptureFileName, this);
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

        public void PlaylistItemDoubleClicked(ListViewItem item)
        {
            MediaItem clickedItem = (MediaItem) (item.DataContext);

            ChangeCurrentMedia(clickedItem);

            //_mediaElement.Source = new Uri(clickedItem.Filepath);
            //_mediaLibrary.SetCurrentMedia(clickedItem);
            //if (_playState == PlayStateEnum.Playing)
            //    _mediaElement.Play();
        }

        public void ShuffleToggled()
        {
            if (_playMode == PlayModeEnum.Consecutive)
            {
                _playMode = PlayModeEnum.Shuffle;
                _view.BTN_playMode.Content = "S";
            }else
            if (_playMode == PlayModeEnum.Shuffle)
            {
                _playMode = PlayModeEnum.Repeat;
                _view.BTN_playMode.Content = "R";
            }
            else
            {
                _playMode = PlayModeEnum.Consecutive;
                _view.BTN_playMode.Content = "C";
            }
        }
        #endregion View Events 



    }
}
