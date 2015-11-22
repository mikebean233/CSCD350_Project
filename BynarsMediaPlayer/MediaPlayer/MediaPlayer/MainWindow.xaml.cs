using System;
using System.Collections.Generic;
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
            this.Dispatcher.Invoke(new Action(() => _mainController.VolumeChanged()), new object[] { });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (new Thread(new ThreadStart(_mainController.Setup))).Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => _mainController.CloseWindow()), new object[] { });
        }
    }
}
