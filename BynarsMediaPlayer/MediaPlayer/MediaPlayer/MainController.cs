using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using Timer = System.Timers.Timer;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Windows.Media.Imaging;

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


        private bool _timerIsChangingScrubBar;
        [DataMember]
        private string _stateCaptureFileName;

        [DataMember]
        private string _currentPlaylistName;

        // This variable is used to change the position of the media, it will be changed when the timer gets triggered
        private double _requestedPositionValue;
        // private List<string> _selectedLibraryFiles;
        private double _playSpeed = 1;
        public List<string> SupportedExtentions { get { return _supportedExtentions; } }
        private List<string> _playlistNames { get; set; }
        private List<MediaLibrary> _playlists { get; set; }
        private MediaLibrary _modifiedPlaylist;
        private MediaLibrary _mediaLibrary;
        private MediaLibrary _currentPlaylist;
        public MainController(MainWindow view)
        {
            _view =  view;
            _supportedExtentions = new List<string>() {"*mp3", "*wma", "*wmv", "*asf"};
            _mediaLibrary = new MediaLibrary("Media Library", this);
            _stateCaptureFileName = "savedState.cfg";
            _playMode = PlayModeEnum.Consecutive;
            _timerIsChangingScrubBar = false;
            _currentItem = new MediaItem();
            _playlistNames = new List<string>();
            _currentPlaylistName = "";
            _currentPlaylist = new MediaLibrary("", this);
            _playlists = new List<MediaLibrary>();
            _mediaElementPollingTimer = new Timer(150);
            _mediaElementPollingTimer.Elapsed += new ElapsedEventHandler(PollingTimerHandler);
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
                if (_currentPlaylistName == "")
                    _currentPlaylistName = "Media Library";
                _currentPlaylist = GetPlaylistByName(_currentPlaylistName);
                SetCurrentPlaylist(_currentPlaylistName);   
                UpdateCurrentPlaylistTab();
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
            _currentPlaylist = _mediaLibrary;
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

            // Get the playlist names
            List<string> playlistNames = _databaseController.getPlaylists();
            if(playlistNames != null && playlistNames.Any() )
                _playlistNames.AddRange(playlistNames);

            //populate the playlists
            foreach (string thisPlaylistName in _playlistNames)
            {
                try
                {
                    List<MediaItem> thisList = _databaseController.retrievePlaylist(thisPlaylistName);
                    if (thisList != null)
                    {
                        MediaLibrary thisPlaylist = new MediaLibrary(thisPlaylistName, this);
                        thisPlaylist.AddRange(thisList);
                        _playlists.Add(thisPlaylist);
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
            }
            
            // If there are no entries in the media library, kick off a thread to scan the filesystem for media, 
            // and add that to the library.
            if (!_mediaLibrary.Any())
            {
                _fileScanner = new FileScanner(this);
                _fileScannerThread = new Thread(new ParameterizedThreadStart(_fileScanner.ScanDirectory));
                _fileScannerThread.Start("C:\\users\\" + Environment.UserName);
            }

            UpdateDataGrids();
        
            // Start the polling timer (which is used to update the view)
            _mediaElementPollingTimer.Start();

            _view.Dispatcher.Invoke(new Action(() => this.UpdatePlayButtonImage()), new object[] { });

            List<MediaItem> tempSongs = _mediaLibrary.GetMedia();

            CreatePlaylist("Mikes Mix");

            AddMediaToPlaylist("Mikes Mix", _mediaLibrary.GetMedia().GetRange(0, 5));

            SetCurrentPlaylist("Mikes Mix");

        }

        private void SetCurrentPlaylist(string playlistName)
        {
            if (String.IsNullOrEmpty(playlistName))
                return;
            _mediaElementPollingTimer.Enabled = false;

            if (playlistName == "Media Library" || !_playlistNames.Contains(playlistName))
            {
                _currentPlaylist = _mediaLibrary;
                _currentPlaylistName = "Media Library";
            }
            else
            {
                foreach (MediaLibrary thisPlaylist in _playlists)
                    if (thisPlaylist.Name == playlistName)
                    {
                        _currentPlaylist = thisPlaylist;
                        _currentPlaylistName = playlistName;
                    }
            }
            UpdateCurrentPlaylistTab();
            ChangeCurrentMedia(_currentPlaylist.GetCurrentMedia());
            _mediaElementPollingTimer.Enabled = true;
        }

        private bool CreatePlaylist(string playlistName)
        {
            if (string.IsNullOrEmpty(playlistName) || _playlistNames.Contains(playlistName) || playlistName == "MediaLibary")
                return false;

            try
            {
                _databaseController.addPlayList(playlistName);
                _playlistNames.Add(playlistName);
                _playlists.Add(new MediaLibrary(playlistName,this));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
       }


        public void UpdateCurrentPlaylistTab()
        {
            _view.Dispatcher.Invoke(new Action(() => _view.TabItem_CurrentPlaylist.Header = _currentPlaylistName), new object[] { });
            UpdateDataGrids();
        }

        public void UpdateDataGrids()
        {
            _view.Dispatcher.Invoke(new Action(() => _view.lv_MediaLibraryView.ItemsSource = _currentPlaylist.GetMedia()), new object[] { });
        }

        public void UpdateContextMenu()
        {
            bool inMediaLibrary = (_currentPlaylistName == "Media Library") ? true : false;
            _view.Dispatcher.Invoke(new Action(() => _view.UpdateContextMenu(_playlistNames, inMediaLibrary)), new object[] { });
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

        public bool AddMediaToPlaylist(string playlistName, List<MediaItem> mediaItems )
        {
            if (String.IsNullOrEmpty(playlistName) || !_playlistNames.Contains(playlistName) || mediaItems == null || !mediaItems.Any())
                return false;

            MediaLibrary thisPlaylist = GetPlaylistByName(playlistName);
            if (thisPlaylist.Name == "")
                return false;

            thisPlaylist.AddRange(mediaItems);

            return true;
        }
        public bool RemoveMediaFromPlaylist(string playlistName, List<MediaItem> mediaItems)
        {
            if (String.IsNullOrEmpty(playlistName) || !_playlistNames.Contains(playlistName) || mediaItems == null || !mediaItems.Any())
                return false;

            MediaLibrary thisPlaylist = GetPlaylistByName(playlistName);
            if (thisPlaylist.Name == "")
                return false;

            foreach (MediaItem thisItem in mediaItems)
                thisPlaylist.RemoveItem(thisItem);

            return true;
        }

        public MediaLibrary GetPlaylistByName(string playlistName)
        {
            if (String.IsNullOrEmpty(playlistName) || !_playlistNames.Contains(playlistName))
                return new MediaLibrary("", this); // return a null object, we should never neeed this

            foreach (MediaLibrary thisPlaylist in _playlists)
                if (thisPlaylist.Name == playlistName)
                    return thisPlaylist;

            return new MediaLibrary("", this); // return a null object, we should never neeed this

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
                UpdateDataGrids();
                _currentItem.Position = completionRatio;

                // update the play mode toggle
                _view.btn_PlayButton.ApplyTemplate();

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



        public void DeleteLibraryEntry(MediaItem thisMedia)
        {
            if (thisMedia == null)
                return;
            RemoveMediaFromPlaylist("Media Library", new List<MediaItem>( new MediaItem[] {thisMedia}) );
        }

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
        }

        public void RemovePlaylist(string playlistName)
        {
            if (String.IsNullOrEmpty(playlistName) || playlistName == "Media Library" || !_playlistNames.Contains(playlistName))
                return;

            _playlistNames.Remove(playlistName);

            _playlists.Remove(GetPlaylistByName(playlistName));

            if(_currentPlaylistName == playlistName)
                SetCurrentPlaylist("Media Library");

            try
            {
                _databaseController.removePlaylist(playlistName);
            }
            catch (Exception e)
            {
            }
        }


        public void FetchMediaLibraryData() { }

        public bool ChangeCurrentMedia(MediaItem _newItem)
        {
            _mediaElementPollingTimer.Enabled = false;
            if (_newItem == null)
                return false;
            try
            {
                if (!_currentPlaylist.SetCurrentMedia(_newItem))
                    return false;
                _currentItem = _currentPlaylist.GetCurrentMedia();
                _mediaElement.Source = new Uri(_currentItem.Filepath);

                _requestedPositionValue = _currentItem.Position;

                if (_playState == PlayStateEnum.Playing)
                    _mediaElement.Play();
            }
            catch (Exception e)
            {
                return false;
            }
            this._currentItem = _currentPlaylist.GetCurrentMedia();
            _mediaElementPollingTimer.Enabled = true;
            return true;
        }


        #region View Events

    /******************************  MAIN BUTTON EVENTS  ******************************/
        public void PauseButtonPressed()
        {
            UpdatePlayButtonImage();
        }

        public void PlayButtonPressed()
        {
            //If currently playing, switch to Paused.
            if(_playState == PlayStateEnum.Playing)
            {
                _mediaElement.Pause();
                _playState = PlayStateEnum.Paused;
            }

            //If currently paused, switch to Playing.
            else
            {
                _mediaElement.Play();
                _playState = PlayStateEnum.Playing;
            }

            UpdatePlayButtonImage();
        }

        public void UpdatePlayButtonImage()
        {
            if (_playState == PlayStateEnum.Paused || _playState == PlayStateEnum.Stopped)
                _view.btn_PlayButton.Source = new BitmapImage(new Uri(@"./Images/PlayButton.png", UriKind.Relative));
            else
                _view.btn_PlayButton.Source = new BitmapImage(new Uri(@"./Images/PauseButton.png", UriKind.Relative));
        }

        public void RewindButtonPressed()
        {
            PlaySpeedCycle("Slower");
        }

        public void SkipBackwardButtonPressed()
        {
            ChangeCurrentMedia(_currentPlaylist.GetPreviousSong());
        }

        public void StopButtonPressed()
        {
            _mediaElement.Stop();
            _playState = PlayStateEnum.Stopped;
            UpdatePlayButtonImage();
        }

        public void SkipForwardButtonPressed()
        {
            Console.WriteLine("Skip Forward");
            ChangeCurrentMedia(_currentPlaylist.GetNextSong());
        }

        public void FastForwardButtonPressed()
        {
            PlaySpeedCycle("Faster");
        }

        public void ShuffleToggled()
        {
            if (_playMode == PlayModeEnum.Consecutive || _playMode == PlayModeEnum.Repeat)
            {
                _playMode = PlayModeEnum.Shuffle;
                _view.BTN_playMode.Content = "S";
            }

            else
            {
                _playMode = PlayModeEnum.Consecutive;
                _view.BTN_playMode.Content = "C";
            }
        }

        public void RepeatToggled()
        {
            if (_playMode == PlayModeEnum.Consecutive || _playMode == PlayModeEnum.Shuffle)
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

        public void PlaySpeedCycle(string speed)
        {
            if (_playSpeed == .25 && speed == "Faster")
            {
                _playSpeed = .5;
                _mediaElement.SpeedRatio = _playSpeed;
            }

            else if (_playSpeed == .5)
            {
                if (speed == "Slower")
                    _playSpeed = .25;
                else
                    _playSpeed = .75;

                _mediaElement.SpeedRatio = _playSpeed;
            }

            else if (_playSpeed == .75)
            {
                if (speed == "Slower")
                    _playSpeed = .5;
                else
                    _playSpeed = 1;

                _mediaElement.SpeedRatio = _playSpeed;
            }

            else if (_playSpeed == 1)
            {
                if (speed == "Slower")
                    _playSpeed = .75;
                else
                    _playSpeed = 1.25;

                _mediaElement.SpeedRatio = _playSpeed;
            }

            else if (_playSpeed == 1.25)
            {
                if (speed == "Slower")
                    _playSpeed = 1;
                else
                    _playSpeed = 1.5;

                _mediaElement.SpeedRatio = _playSpeed;
            }

            else if (_playSpeed == 1.5)
            {
                if (speed == "Slower")
                    _playSpeed = 1.25;
                else
                    _playSpeed = 1.75;

                _mediaElement.SpeedRatio = _playSpeed;
            }

            else if (_playSpeed == 1.75)
            {
                if (speed == "Slower")
                    _playSpeed = 1.5;
                else
                    _playSpeed = 2;

                _mediaElement.SpeedRatio = _playSpeed;
            }

            else if (_playSpeed == 2 && speed == "Slower")
            {
                _playSpeed = 1.75;
                _mediaElement.SpeedRatio = _playSpeed;
            }
        }

        public void ProgressBarMovedByUser(double newValue)
        {
            if (!_timerIsChangingScrubBar)
                _requestedPositionValue = newValue;
        }

        public void VolumeSliderChanged(double newValue)
        {
            _view.me_MediaElement.Volume = newValue;
            _volume = newValue;
        }
        public void PlayListItemMouseLeave(Image image, object item)
        {
            if (item.GetType() == typeof(MediaItem))
            {
                MediaItem thisItem = (MediaItem)item;
                if (_currentItem == null || !_currentItem.Equals(thisItem))
                    ((Image)image).Source = new BitmapImage(new Uri(@"Images\PlayFromListInActive.png", UriKind.Relative));
            }

        }

        public void PlaylistItemClicked(object item)
        {
            if (item.GetType() == typeof(MediaItem))
            {
                MediaItem clickedItem = (MediaItem)item;
                _playState = PlayStateEnum.Playing;
                UpdatePlayButtonImage();
                ChangeCurrentMedia(clickedItem);
            }
        }
        public void PlayListItemMouseEnter(object image, object item)
        {
            if (item.GetType() == typeof(MediaItem))
            {
                MediaItem thisItem = (MediaItem)item;
                if (_currentItem == null || !_currentItem.Equals(thisItem))
                    ((Image)image).Source = new BitmapImage(new Uri(@"Images\PlayFromList.png", UriKind.Relative));
            }
        }

        public void ContextMenuHeaderClicked(string headerValue)
        {
            Console.WriteLine(headerValue);
        }

        public void ContextMenuCreatePlaylist(string playlistName)
        {
            if (!string.IsNullOrEmpty(playlistName) && !_playlistNames.Contains(playlistName))
            {
                _playlistNames.Add(playlistName);
                UpdateContextMenu();
            }

            Console.WriteLine(playlistName);
        }
        public void ContextMenuPlaylistClicked(string playlistName)
        {
            Console.WriteLine(playlistName);
        }



        /******************************  WINDOW EVENTS  ******************************/
        public void CloseWindow()
        {
            Utilities.SerializeObjectToJson<MainController>(_stateCaptureFileName, this);
            _databaseController.AddMediaItemsToDatabase(_mediaLibrary.GetMedia());

            foreach (string thisPlaylistName in _playlistNames)
            {
                MediaLibrary thisPlaylist = GetPlaylistByName(thisPlaylistName);
                _databaseController.AddMediaItemsToDatabase(thisPlaylistName, thisPlaylist.GetMedia());
            }
            
            if (_fileScannerThread != null && _fileScannerThread.IsAlive)
                _fileScannerThread.Abort();
            _mediaElementPollingTimer.Stop();
            _mediaElementPollingTimer.Close();
        }


    /******************************  MEDIA EVENTS  ******************************/
        public void MediaEnded()
        {
            if (_currentItem != null)
                _currentItem.Position = 0.0;

            ChangeCurrentMedia(_currentPlaylist.GetNextSong());
        }

        public void PlaylistItemDoubleClicked(ListViewItem item)
        {
            MediaItem clickedItem = (MediaItem)(item.DataContext);

            ChangeCurrentMedia(clickedItem);
        }

        public void MediaFileError() { }
        
        #endregion View Events 



    }
}
