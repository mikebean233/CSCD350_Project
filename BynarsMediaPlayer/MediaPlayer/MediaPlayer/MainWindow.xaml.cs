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
        private Button[] allButtons = new Button[7];
        
        public MainWindow()
        {
            InitializeComponent();

            _mainController = new MainController(this);

          //Set buttons
            //IEnumerable<Button> allButton = mainScreen.Children.OfType<Button>();
            btn_PlayButton.Visibility = Visibility.Visible;
            btn_PauseButton.Visibility = Visibility.Collapsed;

            allButtons[0] = btn_PlayButton;
            allButtons[1] = btn_PauseButton;
            allButtons[2] = btn_RewindButton;
            allButtons[3] = btn_SkipBackwardButton;
            allButtons[4] = btn_StopButton;
            allButtons[5] = btn_SkipForwardButton;
            allButtons[6] = btn_FastForwardButton;
        }

    /******************************  MAIN BUTTON EVENTS  ******************************/
      //Pause
        private void btn_PauseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button b in allButtons)
                b.Opacity = 0.5;
            btn_PauseButton.Opacity = 1;

            this.Dispatcher.Invoke(new Action(() => _mainController.PauseButtonPressed()), new object[] { });
        }

      //Play
        private void btn_PlayButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button b in allButtons)
                b.Opacity = 0.5;
            btn_PlayButton.Opacity = 1;

            this.Dispatcher.Invoke(new Action(() => _mainController.PlayButtonPressed()), new object[] { });
        }

      //Rewind
        private void btn_RewindButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button b in allButtons)
                b.Opacity = 0.5;
            btn_RewindButton.Opacity = 1;

            this.Dispatcher.Invoke(new Action(() => _mainController.RewindButtonPressed()), new object[] { });
        }

      //SkipBackwards
        private void btn_SkipBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button b in allButtons)
                b.Opacity = 0.5;
            btn_SkipBackwardButton.Opacity = 1;

            this.Dispatcher.Invoke(new Action(() => _mainController.SkipBackwardButtonPressed()), new object[] { });
        }

      //Stop
        private void btn_StopButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button b in allButtons)
                b.Opacity = 0.5;
            btn_StopButton.Opacity = 1;

            this.Dispatcher.Invoke(new Action(() => _mainController.StopButtonPressed()), new object[] { });
        }

      //SkipForward
        private void btn_SkipForwardButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button b in allButtons)
                b.Opacity = 0.5;
            btn_SkipForwardButton.Opacity = 1;

            this.Dispatcher.Invoke(new Action(() => _mainController.SkipForwardButtonPressed()), new object[] { });
        }

      //FastForward
        private void btn_FastForwardButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button b in allButtons)
                b.Opacity = 0.5;
            btn_FastForwardButton.Opacity = 1;

            this.Dispatcher.Invoke(new Action(() => _mainController.FastForwardButtonPressed()), new object[] { });
        }

      //Shuffle
        private void toggleShuffle(object sender, RoutedEventArgs e)
        {
            //shuffleButton.Opacity = 1;
            //repeatButton.Opacity = 0.5;

            Dispatcher.Invoke(new Action(() => _mainController.ShuffleToggled()), new object[] { });
        }

      //Repeat
        private void toggleRepeat(object sender, RoutedEventArgs e)
        {
            //shuffleButton.Opacity = 0.5;
            //repeatButton.Opacity = 1;

            Dispatcher.Invoke(new Action(() => _mainController.RepeatToggled()), new object[] { });
        }

      //ScrubBar
        private void slider_ScrubBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.Invoke(new Action(() => _mainController.ProgressBarMovedByUser(slider_ScrubBar.Value)), new object[] { });
        }

      //Volume
        private void slider_VolumeControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.VolumeSliderChanged(this.slider_VolumeControl.Value)), new object[] { });
        }

      //Hover Buttons
        private void btnBehavior_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;

            if(button.Opacity != 1)
            {
                button.Opacity = .75;
            }
        }

        private void btnBehavior_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;

            if (button.Opacity == .75)
            {
                button.Opacity = .5;
            }
        }
        

    /******************************  WINDOW EVENTS  ******************************/
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (new Thread(new ThreadStart(_mainController.Setup))).Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.CloseWindow()), new object[] { });
        }


    /******************************  MEDIA EVENTS  ******************************/
        private void Me_MediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => _mainController.MediaEnded()), new object[] { });
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(e);
        }



        private void RowImage_OnMouseLeave(object sender, MouseEventArgs e)
        {
            DependencyObject obj = (DependencyObject)e.OriginalSource;

            ListViewItem selectedItem = null;
            while (obj != null && obj != lv_MediaLibraryView)
            {
                if (obj.GetType() == typeof(ListViewItem))
                    Dispatcher.Invoke(new Action(() => _mainController.PlayListItemMouseLeave((Image)sender, ((ListViewItem)obj).DataContext)), new object[] { });

                obj = VisualTreeHelper.GetParent(obj);
            }
        }

        private void RowImage_OnMouseEnter(object sender, MouseEventArgs e)
        {
            DependencyObject obj = (DependencyObject)e.OriginalSource;

            Image sourceImage = null;
            ListViewItem selectedItem = null;
            while (obj != null && obj != lv_MediaLibraryView)
            {

                if (obj.GetType() == typeof(ListViewItem))
                    Dispatcher.Invoke(new Action(() => _mainController.PlayListItemMouseEnter((Image)sender, ((ListViewItem)obj).DataContext)), new object[] { });
                obj = VisualTreeHelper.GetParent(obj);
            }
        }

        private void RowImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = (DependencyObject)e.OriginalSource;

            while (obj != null && obj != lv_MediaLibraryView)
            {
                if (obj.GetType() == typeof(ListViewItem))
                {
                    Dispatcher.Invoke(new Action(() => _mainController.PlaylistItemClicked(((ListViewItem)obj).DataContext)), new object[] { });
                    break;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
        }

        /******************************  INFORMATION EVENTS  ******************************/
        private void HelpBox(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Bynars Media Player" + Environment.NewLine
                            + "Version 1.0" + Environment.NewLine
                            + "by: Micheal Peterson, Travis Heppner, Lexi Guches" + Environment.NewLine
                            + "To use soft ware go to the library tab and add media to the playlist." + Environment.NewLine
                            + "Then hit the play button in order to start the media playlist. " + Environment.NewLine
                            + "The next button will go to the next media, while the previous will" + Environment.NewLine
                            + "go to the previous media.  There is also two sliders to control the" + Environment.NewLine
                            + "position of the media, and the volume of playback.");
        }
    }
}

