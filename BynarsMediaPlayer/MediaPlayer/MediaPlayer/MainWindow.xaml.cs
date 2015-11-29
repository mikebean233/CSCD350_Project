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

        private void poly_PlayButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.PlayButtonPressed()), new object[] { });
        }

        
        private void poly_StopButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.StopButtonPressed()), new object[] { });
        }

        
        private void slider_VolumeControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.VolumeSliderChanged(this.slider_VolumeControl.Value)), new object[] { });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (new Thread(new ThreadStart(_mainController.Setup))).Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.CloseWindow()), new object[] { });
        }

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

        private void DataGrid_MediaL_OnSelected(object sender, RoutedEventArgs e)
        {
            IList items = this.dataGrid_MediaL.SelectedItems;
            this.Dispatcher.Invoke(new Action(() => _mainController.DataGridRowSelected(items)), new object[] { });
        }

        private void poly_SkipBackward_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.SkipBackwardButtonPressed()), new object[] { });
        }

        private void poly_SkipForeward_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.SkipForwardButtonPressed()), new object[] { });
        }

        private void Poly_PauseButton_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => _mainController.PauseButtonPressed()), new object[] { });
        }

        private void slider_ScrubBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.Invoke(new Action(() => _mainController.ProgressBarMovedByUser(slider_ScrubBar.Value)), new object[] { });
        }

        private void Me_MediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => _mainController.MediaEnded()), new object[] { });
        }
    }
}
