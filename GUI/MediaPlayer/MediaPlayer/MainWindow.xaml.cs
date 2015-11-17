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
       //Sound Control
        uint curVolume = 0;
        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);
        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void poly_PlayButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //lbl_MetaData.Content = curMedia.getMetaData();
            //prgBar_ScrubBar.Value = playedTime * 100;
            //lbl_ScrubBarTime.Content = "" playedTime + " / " + totalTime;
        }

        private void poly_StopButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void poly_SkipForeward_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void poly_SkipBackward_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void slider_VolumeControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Calculate Volume
            int newVolume = (int)((ushort.MaxValue / 5) * slider_VolumeControl.Value);

            //Set Volume for all channels
            uint NewVolumeAllChannels = (((uint)newVolume & 0x0000ffff) | ((uint)newVolume << 16));

            //Set the Volume
            waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);

            //Image Change
            if (newVolume == 0)
            {
                //rect_VolumeImage.Fill = imgBrushVolumeOff;
            }

            else
            {
                //rect_VolumeImage.Fill = imgBrushVolumeOn;
            }
        }
    }
}
