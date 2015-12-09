using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ListView = System.Windows.Controls.ListView;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainController _mainController;

        public MainWindow()
        {
            InitializeComponent();

            _mainController = new MainController(this);
        }

        /******************************  MAIN BUTTON EVENTS  ******************************/
        //Play
        private void btn_PlayButton_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.PlayButtonPressed()), new object[] {});
        }

        //Rewind
        private void btn_RewindButton_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.RewindButtonPressed()), new object[] {});
        }

        //SkipBackwards
        private void btn_SkipBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.SkipBackwardButtonPressed()), new object[] {});
        }

        //Stop
        private void btn_StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.StopButtonPressed()), new object[] {});
        }

        //SkipForward
        private void btn_SkipForwardButton_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.SkipForwardButtonPressed()), new object[] {});
        }

        //FastForward
        private void btn_FastForwardButton_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.FastForwardButtonPressed()), new object[] {});
        }

        //Shuffle
        private void toggleShuffle(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => _mainController.ShuffleToggled()), new object[] {});
        }

        //Repeat
        private void toggleRepeat(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => _mainController.RepeatToggled()), new object[] {});
        }

        //ScrubBar
        private void slider_ScrubBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.Invoke(new Action(() => _mainController.ProgressBarMovedByUser(slider_ScrubBar.Value)),
                new object[] {});
        }

        //Volume
        private void slider_VolumeControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Dispatcher.Invoke(
                new Action(() => _mainController.VolumeSliderChanged(this.slider_VolumeControl.Value)), new object[] {});
        }

        //Hover Buttons
        private void btnBehavior_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement element = (FrameworkElement) sender;
            element.Opacity = 0.75;
        }

        private void btnBehavior_MouseLeave(object sender, MouseEventArgs e)
        {
            FrameworkElement element = (FrameworkElement) sender;
            element.Opacity = 0.5;
        }

        private void btnBehavior_MouseDown(object sender, MouseEventArgs e)
        {
            FrameworkElement element = (FrameworkElement) sender;
            element.Opacity = 1.0;
        }

       //
        private void btnBehavior_ChangeImageSource(string buttonNameToSwitchTo)
        {
            if(buttonNameToSwitchTo == "PlayButton")
            {
                btn_PlayButton.Source = new BitmapImage(new Uri(@"./Images/PlayButton.png", UriKind.Relative));
            }

            else if(buttonNameToSwitchTo == "PauseButton")
            {
                btn_PlayButton.Source = new BitmapImage(new Uri(@"./Images/PauseButton.png", UriKind.Relative));
            }
        }



        /******************************  WINDOW EVENTS  ******************************/

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (new Thread(new ThreadStart(_mainController.Setup))).Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.CloseWindow()), new object[] {});
        }


        /******************************  MEDIA EVENTS  ******************************/

        private void Me_MediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => _mainController.MediaEnded()), new object[] {});
        }

        private void RowImage_OnMouseLeave(object sender, MouseEventArgs e)
        {
            DependencyObject obj = (DependencyObject) e.OriginalSource;

            ListViewItem selectedItem = null;
            while (obj != null && obj != lv_MediaLibraryView)
            {
                if (obj.GetType() == typeof (ListViewItem))
                    Dispatcher.Invoke(
                        new Action(
                            () =>
                                _mainController.PlayListItemMouseLeave((Image) sender, ((ListViewItem) obj).DataContext)),
                        new object[] {});

                obj = VisualTreeHelper.GetParent(obj);
            }
        }

        private void RowImage_OnMouseEnter(object sender, MouseEventArgs e)
        {
            DependencyObject obj = (DependencyObject) e.OriginalSource;

            Image sourceImage = null;
            ListViewItem selectedItem = null;
            while (obj != null && obj != lv_MediaLibraryView)
            {

                if (obj.GetType() == typeof (ListViewItem))
                    Dispatcher.Invoke(
                        new Action(
                            () =>
                                _mainController.PlayListItemMouseEnter((Image) sender, ((ListViewItem) obj).DataContext)),
                        new object[] {});
                obj = VisualTreeHelper.GetParent(obj);
            }
        }

        private void RowImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = (DependencyObject) e.OriginalSource;

            while (obj != null && obj != lv_MediaLibraryView)
            {
                if (obj.GetType() == typeof (ListViewItem))
                {
                    Dispatcher.Invoke(
                        new Action(() => _mainController.PlaylistItemClicked(((ListViewItem) obj).DataContext)),
                        new object[] {});
                    break;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
        }

        /******************************  INFORMATION EVENTS  ******************************/

        private void HelpBox(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Bynars Media Player" + Environment.NewLine
                            + "Version 1.99" + Environment.NewLine
                            + "by: Micheal Peterson, Travis Heppner, Lexi Guches" + Environment.NewLine
                            + "To use soft ware go to the library tab and add media to the playlist" + Environment.NewLine
                            + "by right clicking and navigating to the add media to playlist." + Environment.NewLine
                            + "Then select the playlist you want to add from." +
                            Environment.NewLine
                            + "Then hit the play button in order to start the media playlist. " + Environment.NewLine
                            + "The next button will go to the next media, while the previous will" + Environment.NewLine
                            + "go to the previous media.  There is also two sliders to control the" +
                            Environment.NewLine
                            + "position of the media, and the volume of playback.");
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            IList selectedItems = lv_MediaLibraryView.SelectedItems;
            Dispatcher.Invoke(new Action(() => _mainController.ContextMenuHeaderClicked((string) ((MenuItem) sender).Header, selectedItems)), new object[] {});
        }

        private void ContextMenu_CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => _mainController.ContextMenuCreatePlaylist(newPLaylistNameTextbox.Text)), new object[] {});
           // contextMenu.IsOpen = false;
        }

        private void ContextMenu_Playlist_Click(object sender, RoutedEventArgs e)
        {
            IList selectedItems = lv_MediaLibraryView.SelectedItems;
            Dispatcher.Invoke(new Action(() => _mainController.ContextMenuPlaylistClicked((string)((MenuItem)sender).Header, selectedItems)), new object[] { });
        }

        public void UpdateContextMenu(List<string> playlistNames, bool showAddNewMedia)
        {
            if (showAddNewMedia)
            {
                this.contextMenu_MediaItem_AddNewMedia.IsEnabled = true;
                this.contextMenu_MediaItem_AddNewMedia.Visibility = Visibility.Visible;
            }
            else
            {
                this.contextMenu_MediaItem_AddNewMedia.IsEnabled = false;
                this.contextMenu_MediaItem_AddNewMedia.Visibility = Visibility.Collapsed;
            }

            contextMenu_StackPanel_Playlists.Children.Clear();

            if (playlistNames != null && playlistNames.Any())
                contextMenu_Playlist_MenuItem.Visibility = Visibility.Visible;
            else
                contextMenu_Playlist_MenuItem.Visibility = Visibility.Collapsed;

            foreach (string thisPlaylist in playlistNames)
            {
                if (!string.IsNullOrEmpty(thisPlaylist))
                {
                    MenuItem thisItem = new MenuItem() { Header = thisPlaylist };
                    thisItem.Click += new RoutedEventHandler(ContextMenu_Playlist_Click);
                    contextMenu_StackPanel_Playlists.Children.Add(thisItem);
                }
            }


        }

        private void PlaylistDelete_Clicked(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = (DependencyObject)e.OriginalSource;

            while (obj != null && obj != lv_MediaLibraryView)
            {
                if (obj.GetType() == typeof(ListViewItem))
                {
                    try
                    {
                        ListViewItem thisItem = (ListViewItem)obj;
                        string playListName = ((PlaylistViewRow)thisItem.DataContext).Name;

                        Dispatcher.Invoke(
                            new Action(() => _mainController.PlaylistDeleteClicked(playListName)),
                            new object[] { });
                        break;

                    }
                    catch (Exception)
                    {
                    }
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
        }

        private void ListView_PlayList_CLicked(object sender, MouseButtonEventArgs e)
        {
            ListViewItem thisItem = (ListViewItem) sender;
            try
            {
                string selectedPlaylistName = ((PlaylistViewRow)thisItem.DataContext).Name;
                Dispatcher.Invoke(new Action(() => _mainController.PlaylistSelected(selectedPlaylistName)), new object[] { });
             }
            catch (Exception ex)
            {
            }
        }

        private void SearchImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => _mainController.SearchClicked(SearchText.Text)), new object[] { });
        }

        private void SearchText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => _mainController.SearchClicked(SearchText.Text)), new object[] { });
        }
    }
}

