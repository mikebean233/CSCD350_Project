using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
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
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using MessageBox = System.Windows.MessageBox;

namespace MediaPlayer
{
    [DataContract]
    public class MainController
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
        private string _defaultScanDirectory;

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
        private List<MediaList> _playlists { get; set; }
        private MediaList _modifiedPlaylist;
        private MediaList _mediaLibrary;

        private MediaList _currentPlaylist;
        public MainController(MainWindow view)
        {
            _view =  view;
            _supportedExtentions = new List<string>() {"*mp3", "*wma", "*wmv"};
            _mediaLibrary = new MediaList("Media Library", this);
            _stateCaptureFileName = "savedState.cfg";
            _playMode = PlayModeEnum.Consecutive;
            _timerIsChangingScrubBar = false;
            _currentItem = new MediaItem();
            _playlistNames = new List<string>();
            _currentPlaylistName = "";
            _currentPlaylist = new MediaList("", this);
            _playlists = new List<MediaList>();
            _mediaElementPollingTimer = new Timer(150);
            _mediaElementPollingTimer.Elapsed += new ElapsedEventHandler(PollingTimerHandler);
            _fileScanner = new FileScanner(this);
            _fileScannerThread = new Thread(new ParameterizedThreadStart(_fileScanner.ScanDirectory));
            _defaultScanDirectory = "C:\\users\\" + Environment.UserName;
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
                        MediaList thisPlaylist = new MediaList(thisPlaylistName, this);
                        thisPlaylist.Deletable = true;
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
                StartScanner(_defaultScanDirectory, false);
            }

            // Get the player back to the playlist and media it was at before the last shut down
            _currentPlaylist = GetPlaylistByName(_currentPlaylistName);
            _currentPlaylist.SetCurrentMedia(_currentItem);
            SetCurrentPlaylist(_currentPlaylistName);
            ChangeCurrentMedia(_currentPlaylist.GetCurrentMedia());

            UpdateDataGrids();
            UpdateContextMenu();
            _view.Dispatcher.Invoke(new Action(() => UpdatePlaylistsTab()), new object[] { });


            // Start the polling timer (which is used to update the view)
            _mediaElementPollingTimer.Start();

        }
        
        private bool StartScanner(string directory, bool recurse)
        {
            if (String.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return false;
            if (_fileScannerThread.IsAlive)
                return false;
            if(_fileScannerThread.ThreadState == ThreadState.Unstarted)
                _fileScannerThread.Start(new FileScanner.FileScannerStartParameters() { InitialDirectory = directory, Recurse = recurse} );
            else if (_fileScannerThread.ThreadState == ThreadState.Stopped)
            {
                _fileScannerThread = new Thread(new ParameterizedThreadStart(_fileScanner.ScanDirectory));
                _fileScannerThread.Start(new FileScanner.FileScannerStartParameters() {InitialDirectory = directory, Recurse = recurse});
            }
            return true;
        }

        private void SetCurrentPlaylist(string playlistName)
        {
            if (String.IsNullOrEmpty(playlistName))
                return;
            _mediaElementPollingTimer.Enabled = false;


            // Check to see if we are showing results of a search or sort, this is done by seeing if _modifiedPlaylist is null
            if (_modifiedPlaylist != null)
            {
                UpdateCurrentPlaylistTab();
                UpdateDataGrids();
                ChangeCurrentMedia(_modifiedPlaylist.GetCurrentMedia());
                _mediaElementPollingTimer.Enabled = true;
                return;
            }


            // Check to see if we are changing back to he Media Library
            if (playlistName == "Media Library" || !_playlistNames.Contains(playlistName))
            {
                _currentPlaylist = _mediaLibrary;
                _currentPlaylistName = "Media Library";
            }
            else
            {
                foreach (MediaList thisPlaylist in _playlists)
                    if (thisPlaylist.Name == playlistName)
                    {
                        _currentPlaylist = thisPlaylist;
                        _currentPlaylistName = playlistName;
                    }
            }
            UpdateDataGrids();
            UpdateCurrentPlaylistTab();
            UpdateContextMenu();
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
                _playlists.Add(new MediaList(playlistName,this) {Deletable = true});
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
            List<MediaItem> gridItems;
            if (_modifiedPlaylist == null)
                gridItems = _currentPlaylist.GetMedia();
            else
                gridItems = _modifiedPlaylist.GetMedia();
            _view.Dispatcher.Invoke(new Action(() => _view.lv_MediaLibraryView.ItemsSource = gridItems), new object[] { });
            _view.Dispatcher.Invoke(new Action(UpdatePlaylistsTab), new object[] { });
        }

        public void UpdatePlaylistsTab()
        {
            _view.lv_Playlists.Items.Clear();
            _view.lv_Playlists.Items.Add(new PlaylistViewRow() {Name = "Media Library", Deletable = false});

            foreach (string thisPlaylist in _playlistNames)
                _view.lv_Playlists.Items.Add(new PlaylistViewRow() {Name = thisPlaylist, Deletable = true});
        }


        public void UpdateContextMenu()
        {
            bool inMediaLibrary = _currentPlaylistName == "Media Library";
            List<string> modifiedPlaystNames = _playlistNames.ToList();
            modifiedPlaystNames.Remove(_currentPlaylistName);
            _view.Dispatcher.Invoke(new Action(() => _view.UpdateContextMenu(modifiedPlaystNames, inMediaLibrary)), new object[] { });
        }

        public void UpdatePlayModeButtons()
        {
            switch (_playMode)
            {
                case PlayModeEnum.Consecutive:
                    _view.btn_ShuffleButton.Source = new BitmapImage(new Uri(@"Images\ShuffleButton.png", UriKind.Relative));
                    _view.btn_RepeatButton.Source = new BitmapImage(new Uri(@"images\RepeatButton.png", UriKind.Relative));
                    break;
                case PlayModeEnum.Repeat:
                    _view.btn_ShuffleButton.Source = new BitmapImage(new Uri(@"Images\ShuffleButton.png", UriKind.Relative));
                    _view.btn_RepeatButton.Source = new BitmapImage(new Uri(@"Images\RepeatButtonActive.png", UriKind.Relative));
                    break;
                case PlayModeEnum.Shuffle:
                    _view.btn_ShuffleButton.Source = new BitmapImage(new Uri(@"Images\ShuffleButtonActive.png", UriKind.Relative));
                    _view.btn_RepeatButton.Source =  new BitmapImage(new Uri(@"Images\RepeatButton.png", UriKind.Relative));
                    break;
            }
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
            if (String.IsNullOrEmpty(playlistName) || (!_playlistNames.Contains(playlistName) && playlistName != "Media Library") || mediaItems == null || !mediaItems.Any())
                return false;

            MediaList thisPlaylist = GetPlaylistByName(playlistName);
            if (thisPlaylist.Name == "")
                return false;

            thisPlaylist.AddRange(mediaItems);
            try
            {
                _databaseController.AddMediaItemsToDatabase(playlistName, mediaItems);
            }
            catch (Exception e )
            {
            }
            return true;
        }
        public bool RemoveMediaFromPlaylist(string playlistName, List<MediaItem> mediaItems)
        {
            if (String.IsNullOrEmpty(playlistName) || (!_playlistNames.Contains(playlistName) && playlistName != "Media Library") || mediaItems == null || !mediaItems.Any())
                return false;

            MediaList thisPlaylist = GetPlaylistByName(playlistName);
            if (thisPlaylist.Name == "")
                return false;

            foreach (MediaItem thisItem in mediaItems)
            {
                thisPlaylist.RemoveItem(thisItem);
                _databaseController.remove(playlistName, mediaItems);
            }
            return true;
        }

        public MediaList GetPlaylistByName(string playlistName)
        {
            if (String.IsNullOrEmpty(playlistName) || (!_playlistNames.Contains(playlistName) && playlistName != "Media Library"))
                return new MediaList("", this); // return a null object, we should never neeed this

            if (playlistName == "Media Library")
                return _mediaLibrary;

            foreach (MediaList thisPlaylist in _playlists)
                if (thisPlaylist.Name == playlistName)
                    return thisPlaylist;

            return new MediaList("", this); // return a null object, we should never neeed this

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
               // UpdateDataGrids();
                _currentItem.Position = completionRatio;
            }
            else
            {
                _view.lbl_ScrubBarTime.Content = "0:00:00/0:00:00";
                _view.slider_ScrubBar.Value = 0;
            }

            //Update the Volume Slider
            _view.slider_VolumeControl.Value = _volume;

            // Update the play/pause button image
            UpdatePlayButtonImage();

            // Update the play mode buttons
            UpdatePlayModeButtons();

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
                AddMediaToPlaylist("Media Library", new List<MediaItem>(new MediaItem[] {thisItem}));
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
            MediaList displayedPlaylist = (_modifiedPlaylist == null) ? _currentPlaylist : _modifiedPlaylist;

            _mediaElementPollingTimer.Enabled = false;
            if (_newItem == null)
                return false;
            try
            {
                if (!displayedPlaylist.SetCurrentMedia(_newItem))
                    return false;
                _currentItem = displayedPlaylist.GetCurrentMedia();
                _mediaElement.Source = new Uri(_currentItem.Filepath);

                _requestedPositionValue = _currentItem.Position;

                switch (_playState)
                {
                    case PlayStateEnum.Paused:
                        _mediaElement.Pause();
                        break;
                    case PlayStateEnum.Playing:
                        _mediaElement.Play();
                        break;
                    case PlayStateEnum.Stopped:
                        _mediaElement.Stop();
                        break;
                }

            }
            catch (Exception e)
            {
                return false;
            }
            this._currentItem = displayedPlaylist.GetCurrentMedia();
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
            MediaList displayedPlaylist = (_modifiedPlaylist == null) ? _currentPlaylist : _modifiedPlaylist;
            ChangeCurrentMedia(displayedPlaylist.GetPreviousSong());
            UpdateDataGrids();
        }

        public void StopButtonPressed()
        {
            _mediaElement.Stop();
            _playState = PlayStateEnum.Stopped;
            UpdatePlayButtonImage();
        }

        public void SkipForwardButtonPressed()
        {
            MediaList displayedPlaylist = (_modifiedPlaylist == null) ? _currentPlaylist: _modifiedPlaylist;
            ChangeCurrentMedia(displayedPlaylist.GetNextSong());
            UpdateDataGrids();
        }

        public void FastForwardButtonPressed()
        {
            PlaySpeedCycle("Faster");
        }

        public void ShuffleToggled()
        {
            if (_playMode == PlayModeEnum.Consecutive || _playMode == PlayModeEnum.Repeat)
                _playMode = PlayModeEnum.Shuffle;
            else
                _playMode = PlayModeEnum.Consecutive;

            UpdatePlayModeButtons();
        }

        public void RepeatToggled()
        {
            if (_playMode == PlayModeEnum.Consecutive || _playMode == PlayModeEnum.Shuffle)
                _playMode = PlayModeEnum.Repeat;
            else
                _playMode = PlayModeEnum.Consecutive;

            UpdatePlayModeButtons();
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
            UpdateDataGrids();
        }

        public void PlaylistDeleteClicked(string playlistName)
        {
            MediaList clickedItem = GetPlaylistByName(playlistName);      
            if (_currentPlaylistName == playlistName)
                SetCurrentPlaylist("MediaLibrary");
            RemovePlaylist(clickedItem.Name);
            UpdateDataGrids();    
        }

        public void PlaylistSelected(string playlistName)
        {
            if(!String.IsNullOrEmpty(playlistName))
                SetCurrentPlaylist(playlistName);
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

        public void SearchClicked(string searchValue)
        {
            if (String.IsNullOrEmpty(searchValue))
            {
                _modifiedPlaylist = null;
                SetCurrentPlaylist(_currentPlaylistName);
                return;
            }
        
            List<MediaItem> searchResults;

            List<TagType> searchColumns = new List<TagType>(new TagType[] { TagType.Title, TagType.Album, TagType.Artist, TagType.Genre });

            if (_currentPlaylistName == "Media Library")
                searchResults = _databaseController.search(searchValue, searchColumns );
            else
                searchResults = _databaseController.search(_currentPlaylistName, searchValue, searchColumns);

            if (searchResults != null && searchResults.Any())
            {
                _modifiedPlaylist = new MediaList(_currentPlaylistName, this);
                _modifiedPlaylist.AddRange(searchResults);
                SetCurrentPlaylist(_currentPlaylistName);    
            }
        }
        
        public void ContextMenuHeaderClicked(string headerValue, IList selectedItems)
        {
            List<MediaItem> newItems = new List<MediaItem>();
            foreach (object thisItem in selectedItems)
                newItems.Add((MediaItem)thisItem);

            if (String.IsNullOrEmpty(headerValue))
                return;
            switch (headerValue)
            {
                case "Delete Selection":
                    RemoveMediaFromPlaylist(_currentPlaylistName, newItems);
                    break;
                case "Add Selection to Playlist":
                    AddMediaToPlaylist(_currentPlaylistName, newItems);
                    break;
                case "Add Files":
                    OpenFileDialog newFilesDialog = new OpenFileDialog(){Filter = "mp3 |*.mp3|wma |*.wma|wmv |*.wmv", Multiselect = true};
                    newFilesDialog.ShowDialog();

                    if (newFilesDialog.FileNames.Any())
                    {
                        foreach(string thisFilePath in newFilesDialog.FileNames)
                            AddMediaEvent(thisFilePath);
                    }
                    
                    UpdateDataGrids();
                    break;
                case "Add Files In Directory":
                    FolderBrowserDialog singleFolderDialog = new FolderBrowserDialog() { Description = "Select a media folder"};
                    singleFolderDialog.ShowDialog();

                    if (!String.IsNullOrEmpty(singleFolderDialog.SelectedPath))
                        if (!StartScanner(singleFolderDialog.SelectedPath, false))
                            MessageBox.Show(
                                "The program is Scanning for files right now, please try again after the current scan as fished");
                    break;
                case "Add Files In Directory (Include Subdirectories)":
                    FolderBrowserDialog multipleFolderDialog = new FolderBrowserDialog() { Description = "Select a media folder" };
                    multipleFolderDialog.ShowDialog();

                    if (!String.IsNullOrEmpty(multipleFolderDialog.SelectedPath))
                        if (!StartScanner(multipleFolderDialog.SelectedPath, true))
                            MessageBox.Show(
                                "The program is Scanning for files right now, please try again after the current scan as fished");
                    break;
                    
            }
            UpdateDataGrids();
            UpdateContextMenu();
        }

        public void ContextMenuCreatePlaylist(string playlistName)
        {
            CreatePlaylist(playlistName);
            UpdateContextMenu();

            Console.WriteLine(playlistName);
        }
        public void ContextMenuPlaylistClicked(string playlistName, IList selectedItems)
        {
            List<MediaItem> newItems = new List<MediaItem>();
            foreach(object thisItem in selectedItems)
                newItems.Add((MediaItem)thisItem);

            AddMediaToPlaylist(playlistName, newItems);
            Console.WriteLine(playlistName);
        }



        /******************************  WINDOW EVENTS  ******************************/
        public void CloseWindow()
        {
            Utilities.SerializeObjectToJson<MainController>(_stateCaptureFileName, this);
            _databaseController.AddMediaItemsToDatabase(_mediaLibrary.GetMedia());

            foreach (string thisPlaylistName in _playlistNames)
            {
                MediaList thisPlaylist = GetPlaylistByName(thisPlaylistName);
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

            MediaList displayedPlaylist = (_modifiedPlaylist == null) ? _currentPlaylist : _modifiedPlaylist;
            ChangeCurrentMedia(displayedPlaylist.GetNextSong());
            UpdateDataGrids();
        }

        public void MediaFileError()
        {
            MediaList displayedPlaylist = (_modifiedPlaylist == null) ? _currentPlaylist : _modifiedPlaylist;
            ChangeCurrentMedia(displayedPlaylist.GetNextSong());
        }

        #endregion View Events 
    }
}
