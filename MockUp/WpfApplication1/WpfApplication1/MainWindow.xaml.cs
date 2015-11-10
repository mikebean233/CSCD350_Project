using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
using Microsoft.Win32;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer player;
        public MainWindow()
        {
            InitializeComponent();
            player = new MediaPlayer();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();

            player.Open(new Uri(ofd.FileName));
            //ofd.FileOk += fileOpenOk;

        }

        private void fileOpenOk(object sender, CancelEventArgs e)
        {
            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            player.Play();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            player.Pause();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            player.Pause();
        }
    }
}
