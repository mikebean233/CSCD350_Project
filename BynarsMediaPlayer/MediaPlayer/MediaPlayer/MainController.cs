using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Timers;
using System.Windows;
using Timer = System.Timers.Timer;
using ICSharpCode;
using ICSharpCode.SharpZipLib;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Id3Lib;
using Mp3Lib;
namespace MediaPlayer
{

    class MainController
    {
        private MainWindow _view;
        private string _currentMedia;
        private int _volume; // 1 - 100
        private PlayModeEnum _playMode;

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

        // private List<string> _selectedLibraryFiles;
         
        public List<string> SupportedExtentions { get { return _supportedExtentions; } } 

        public MainController(MainWindow view)
        {
            _view =  view;
            _supportedExtentions = new List<string>() {"*mp3", "*wma", "*wmv", "*asf"};
            _mediaLibrary = new MediaLibrary("Media Library", this);
        }

        public void Setup()
        {
            _mediaElement = _view.me_MediaElement;
            _mediaElementPollingTimer = new Timer(500); // Create a timer that polls every 1/2 second
            _mediaElementPollingTimer.Elapsed += new ElapsedEventHandler(PollingTimerHandler);
            _mediaElementPollingTimer.Start();

            _databaseController = new DatabaseController();
            _mediaLibrary.AddRange(_databaseController.GetMediaItemsFromDatabase());
            if (!_mediaLibrary.Any())
            {
                _fileScanner = new FileScanner(this);
                _fileScannerThread = new Thread(new ParameterizedThreadStart(_fileScanner.ScanDirectory));
                _fileScannerThread.Start("C:\\");
            }

            /**
             *   TODO:
             *      1) load serialized state from savedState.cfg (in current directory)
             *         - If error, setup default state and create/correct savedState.cfg
             *     
             *      2) Instantiate database controller
             *         - Check to see of if there is a media library database, if not create one.
             *
             *      3) Check to see if there are records in the database
             *         a) There are records: 
             *            - Populate the media library control (the DataGridView)
             *            - Load the last selected song as described in savedState.cfg (if the song is in the library)
             *         b) There are no records: 
             *            - Scan the entire file system for media files, as files are found, update the view as appropriate.           
             *
             *      2) Load the playlist data from the file ??????????? and update the view appropiately, 
             *           
             *      3) st 
             */
        }

        public void UpdateDataGrids()
        {
            
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
            CheckMediaObjectStatus();
        }

        public void CheckMediaObjectStatus()
        {
           // Console.WriteLine("Check Status");
            if (_mediaElement != null)
            {
                // TODO: Update the view (the progress bar and timer)
                //       Check to see if the media has finished, in which case move on the the next song (using the current play mode)
            }

        }

        
        public void DeleteLibraryEntry() { }

        // This method is called by the FileScanner 
        // (or possibly another piece of code) when a new file needs to be added to the library
        public void AddMediaEvent(string newMediaPath)
        {
            if (string.IsNullOrEmpty(newMediaPath))
                return;
            MediaItem thisItem = Utilities.BuildMediaItemFromPath(newMediaPath);
            if (thisItem != null)
            {
                Console.WriteLine("Count: " + _mediaLibrary.AddNewMediaItem(thisItem));
               // _view.dataGrid_MediaL.DataContext = _mediaLibrary.GetMedia();
                _view.Dispatcher.Invoke(new Action(() => _view.dataGrid_MediaL.DataContext = _mediaLibrary.GetMedia()), new object[] { });
            }
            //_view.Dispatcher.Invoke(new Action(() => _databaseController.retrievePlaylistToDataGrid(_view.dataGrid_MediaL)), new object[] { });
        }

        public void FetchMediaLibraryData() { }

        #region View Events

        public void SkipForwardButtonPressed()
        {
            Console.WriteLine("Skip Forward");
        }

        public void SkipBackwardButtonPressed()
        {
            Console.WriteLine("Skip Backward");
        }

        public void VolumeSliderChanged(double newValue)
        {
            Console.WriteLine("Volume Value: " + newValue);
        }

        public void PlayButtonPressed()
        {
            Console.WriteLine("Play");
        }
        public void StopButtonPressed()
        {
            Console.WriteLine("Stop");
        }

        public void PauseButtonPressed()
        {
            Console.WriteLine("Pause");
        }

        public void ProgressBarMovedByUser(double newValue)
        {
            Console.WriteLine("ProgressBar Value: " + newValue);
        }

        public void CloseWindow()
        {
            //Console.WriteLine("Close");
            _databaseController.AddMediaItemsToDatabase(_mediaLibrary.GetMedia());
            if(_fileScannerThread != null && _fileScannerThread.IsAlive)
                _fileScannerThread.Abort();
            _mediaElementPollingTimer.Stop();
            _mediaElementPollingTimer.Close();
        }
        public void MediaCompleted() { }
        public void MediaFileError() { }

        public void DataGridRowSelected(IList items)
        {
            //_selectedLibraryFiles = new List<string>();
            //foreach(System.Data.DataRowView thisRow in items)
            //    _selectedLibraryFiles.Add((string)thisRow.Row.ItemArray[1]);
        }
        
        #endregion View Events 



    }
}
