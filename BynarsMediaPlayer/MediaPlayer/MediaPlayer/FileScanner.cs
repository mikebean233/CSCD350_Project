using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Windows.Documents;

namespace MediaPlayer
{


    class FileScanner
    {
        #region FileScannerStartParameters
        public class FileScannerStartParameters
        {
            private string _initialDirectory;
            private bool _recurse;

            public string InitialDirectory
            {
                get { return _initialDirectory; }
                set { if (!String.IsNullOrEmpty(value)) _initialDirectory = value; }
            }

            public bool Recurse
            {
                get { return _recurse; }
                set { _recurse = value; }
            }

            public FileScannerStartParameters()
            {
                _initialDirectory = "";
                _recurse = false;
            }

        }

        #endregion

        private static MainController _mainController;

        public FileScanner(MainController mainController)
        {
            _mainController = mainController;
        }

        public void ScanDirectory(object par)
        {
            FileScannerStartParameters parameters = (FileScannerStartParameters) par;

            retreiveAllList(parameters.InitialDirectory, _mainController.SupportedExtentions, parameters.Recurse);
        }


        /// <summary>
        /// takes a directory and returns the file paths for all files with the same type
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="type"></param>
        /// <returns>array of file paths</returns>
        public static string[] retreieveFilesArray(string directory, string type, bool recurse)
        {
            string[] files = new string[0];

            //Console.Out.WriteLine("retrieving files in " + dir);
            try
            {
                files = Directory.GetFiles(directory, type, SearchOption.AllDirectories);
            }
            catch (UnauthorizedAccessException e)
            {
                try
                {
                    files = Directory.GetFiles(directory, type, SearchOption.TopDirectoryOnly);
                }
                catch (UnauthorizedAccessException ex)
                {
                    return files;
                }
                string[] directories = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly);

                if (recurse)
                {
                    foreach (string subdir in directories)
                    {
                        files = appendStringArray(files, retreieveFilesArray(subdir, type, recurse));
                    }
                }
            }
            catch (Exception e)
            {
                //Console.Out.Write("" + e);
            }

            return files;
        }

        /// <summary>
        /// takes a directory and returns the file paths for all files with the same type
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="type"></param>
        /// <returns>List of file paths</returns>
        public static ObservableCollection<string> retreieveFilesList(string directory, string type, bool recurse)
        {
            if (string.IsNullOrEmpty(directory))
                return new ObservableCollection<string>();

            ObservableCollection<string> files = new ObservableCollection<string>();
            files.CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChanged);
            try
            {
                string[] theseFiles = Directory.GetFiles(directory, type, SearchOption.TopDirectoryOnly);
                //files.InsertRange(files.Count, Directory.GetFiles(directory, type, SearchOption.TopDirectoryOnly));
                if (recurse)
                {
                    foreach (string thisDir in theseFiles)
                    {
                        files.Add(thisDir);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {

                return files;
            }
            string[] directories = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly);

            foreach (string subdir in directories)
            {
                //files.AddRange(retreieveFilesList(subdir, type));
                Collection<string> theseFiles = retreieveFilesList(subdir, type, recurse);
                foreach (string thisFile in theseFiles)
                {
                    files.Add(thisFile);
                }

            }

            return files;
        }

        private static void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach(string thisFile in e.NewItems)
                _mainController.AddMediaEvent(thisFile);
        }

        private static string[] appendStringArray(string[] a, string[] b)
        {
            string[] temp = new string[a.Length + b.Length];
            a.CopyTo(temp, 0);
            b.CopyTo(temp, a.Length);
            return temp;
        }

        /// <summary>
        /// takes a directory and a list of filestypes and returns all files with that extension in the directory
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="type"></param>
        /// <returns> a list of all filetypes in that directory</returns>
        public static List<string> retreiveAllList(string directory, IEnumerable<string> type, bool recurse)
        {
            List<string> s = new List<string>();
            foreach (string t in type)
            {
                s.AddRange(retreieveFilesList(directory, t, recurse));
            }
            return s;
        }
    }

}

