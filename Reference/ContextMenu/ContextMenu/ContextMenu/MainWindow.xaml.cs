using System;
using System.Collections.Generic;
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

namespace ContextMenu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            for (int i = 1; i < 5; ++i)
            {
                MenuItem thisItem = new MenuItem() { Header = "PlayList " + i };
                thisItem.Click += new RoutedEventHandler(MenuItem_Click);
                contextMenu_StackPanel_Playlists.Children.Add(thisItem);    
            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(((MenuItem)sender).Header);
        }
    }
}
