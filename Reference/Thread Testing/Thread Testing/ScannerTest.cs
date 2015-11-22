using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thread_Testing
{
    class ScannerTest
    {
        private MainWindow _parentWindow;

        public ScannerTest(MainWindow parentWindow)
        {
            _parentWindow = parentWindow;
        }

        public void RunTest()
        {
            for (int i = 0; i < 20; ++i)
            {
                _parentWindow.Dispatcher.Invoke(new Action(() => _parentWindow.AppendText(" " + i + " ")), new object[] { });
                
                System.Threading.Thread.Sleep(1000);
            }

        }

    }
}
