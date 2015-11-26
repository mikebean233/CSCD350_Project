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
       //Variables
        String mediaSource = "";

        public MainWindow()
        {
            InitializeComponent();

           //Set a Default Song for Testing.
            mediaSource = "C:/Users/Lexi/Documents/2015-2016/SoftwareEngineering/MediaPlayer/TestingFile/Axwell Λ Ingrosso - On My Way.mp3";
            me_MediaElement.Source = new Uri(mediaSource);
        }

        private void poly_PlayButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            me_MediaElement.Play();
            //lbl_MetaData.Content = curMedia.getMetaData();
            //prgBar_ScrubBar.Value = playedTime * 100;
            //lbl_ScrubBarTime.Content = "" playedTime + " / " + totalTime;
        }

        private void poly_StopButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            me_MediaElement.Stop();
        }

        private void poly_SkipForeward_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void poly_SkipBackward_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void slider_VolumeControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Set the Volume
            me_MediaElement.Volume = (double)slider_VolumeControl.Value;

            //Image Change For Using Icons
            if (slider_VolumeControl.Value == 0)
            {
                //rect_VolumeImage.Fill = imgBrushVolumeOff;
            }

            else
            {
                //rect_VolumeImage.Fill = imgBrushVolumeOn;
            }
        }

        private void changePlayingSpeed(object sender, MouseButtonEventArgs e)
        {
            int defaultValue = 1;

            me_MediaElement.SpeedRatio = (double)defaultValue;
        }
    }
}
