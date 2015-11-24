using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace MediaPlayer
{
    class FileScanner
    {
        private static MainController _mainController;

        public FileScanner(MainController mainController)
        {
            _mainController = mainController;
        }

        public void ScanDirectory(object path)
        {
            // add some pretend files to the library
            //_mainController.AddMediaEvent("C:\\PretendFile1.mp3");
            //_mainController.AddMediaEvent("C:\\PretendFile2.mp3");
            retreiveAllList((string) path, _mainController.SupportedExtentions);
        }

        /*
        static void Main(string[] args)
        {
            string directory = "c:\\";//Directory.GetCurrentDirectory();
            string type = "*.txt";
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();
            string[] files = new string[0];
            files = retreieveFilesArray(directory, type);
            s.Stop();
            Console.Out.WriteLine(s.Elapsed);
            //foreach (string d in files)
            //{
            //    Console.Out.WriteLine(d);
            //}
            Console.Out.WriteLine("Press enter to exit.");
            Console.Read();
        }
        */

        /// <summary>
        /// takes a directory and returns the file paths for all files with the same type
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="type"></param>
        /// <returns>array of file paths</returns>
        public static string[] retreieveFilesArray(string directory, string type)
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

                foreach (string subdir in directories)
                {
                    files = appendStringArray(files, retreieveFilesArray(subdir, type));
                }

            }
            catch (Exception e)
            {
                Console.Out.Write("" + e);
            }

            return files;
        }

        /// <summary>
        /// takes a directory and returns the file paths for all files with the same type
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="type"></param>
        /// <returns>List of file paths</returns>
        public static ObservableCollection<string> retreieveFilesList(string directory, string type)
        {
            ObservableCollection<string> files = new ObservableCollection<string>();
            files.CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChanged);
            try
            {
                string[] theseFiles = Directory.GetFiles(directory, type, SearchOption.TopDirectoryOnly);
                //files.InsertRange(files.Count, Directory.GetFiles(directory, type, SearchOption.TopDirectoryOnly));
                foreach(string thisDir in theseFiles)
                {
                    files.Add(thisDir);
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
                Collection<string> theseFiles = retreieveFilesList(subdir, type);
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
        public static List<string> retreiveAllList(string directory, IEnumerable<string> type)
        {
            List<string> s = new List<string>();
            foreach (string t in type)
            {
                s.AddRange(retreieveFilesList(directory, t));
            }
            return s;
        }

        /// <summary>
        /// finds all filePaths for every file matching the extensions in types in all given directories
        /// </summary>
        /// <param name="directories"></param>
        /// <param name="types"></param>
        /// <returns> a List of all file pathes</returns>
        public static List<string> retreiveAllFromAll(IEnumerable<string> directories, IEnumerable<string> types)
        {
            List<string> s = new List<string>();

            //checkListForNestedDirectories(directories);

            foreach (string directory in directories)
            {
                s.AddRange(retreiveAllList(directory, types));
            }
            return s;
        }


    }

}

