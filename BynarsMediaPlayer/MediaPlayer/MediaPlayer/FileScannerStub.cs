using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    class FileScannerStub
    {
        private MainController _mainController;

        public FileScannerStub(MainController mainController)
        {
            _mainController = mainController;
        }

        public void ScanDirectory(object path)
        {
            // add some pretend files to the library
            _mainController.AddMediaEvent("C:\\PretendFile1.mp3");
            _mainController.AddMediaEvent("C:\\PretendFile2.mp3");
        }

        

    }
}
