using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Thread_Testing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            ScannerTest scannerTest = new ScannerTest(this);

            ThreadStart threadStart = new ThreadStart(scannerTest.RunTest);
            Thread thread = new Thread(threadStart);
            thread.Start();
            //this.Dispatcher.Invoke(new Action(() => (new ScannerTest(this)).RunTest()), new object[] { });
        }

        public void AppendText(string newText)
        {
            this.Dispatcher.Invoke(new Action(() => this.textBlock.Text += newText), new object[] { });
            
        }

    }
}
