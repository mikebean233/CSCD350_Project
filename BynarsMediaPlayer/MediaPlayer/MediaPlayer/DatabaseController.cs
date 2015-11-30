using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Data;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Automation.Peers;

namespace MediaPlayer
{
    class DatabaseController
    {
            private SQLiteConnection sqlConnection;
            private SQLiteCommand sqlCommand;

            public DatabaseController()
            {
                if (!File.Exists("media.DB"))
                {
                    SQLiteConnection.CreateFile("media.sqlite");
                }

                sqlConnection = new SQLiteConnection("Data Source = media.DB;Version = 3");
                sqlConnection.Open();
                String sql = "CREATE TABLE IF NOT EXISTs library (FileName VARCHAR, Path VARCHAR UNIQUE, FileType VARCHAR, Title VARCHAR, Duration VARCHAR, Artist VARCHAR, Album VARCHAR, Position VARCHAR) ";
                sqlCommand = new SQLiteCommand(sql, sqlConnection);
                sqlCommand.ExecuteNonQuery();
        }

        public List<MediaItem> GetMediaItemsFromDatabase()
        {
            List<MediaItem> items = new List<MediaItem>();
            sqlCommand.CommandText = "SELECT * FROM library";
            SQLiteDataReader reader = sqlCommand.ExecuteReader();//(new SQLiteCommand("SELECT * FROM library", sqlConnection)).ExecuteReader();
            
            while (reader.Read())
            {
                try
                {
                    MediaItem thisItem = new MediaItem((string) reader["Path"])
                    {
                        Album    = (string) reader["Album"],
                        Artist   = (string) reader["Artist"],
                        Duration = (long)   reader["Duration"],
                        Filename = (string) reader["FileName"],
                        Filetype = (string) reader["FileType"],
                        Position = (double) reader["Position"],
                        Title    = (string) reader["Title"]
                    };
                    items.Add(thisItem);
                }
                catch (Exception e)
                {
                    
                }
            }// end while
            reader.Dispose();
            return items;
        }

        public void AddMediaItemsToDatabase(ICollection<MediaItem> items)
        {
            if (items == null || !items.Any())
                return;
            foreach(MediaItem thisItem in items)
                addToLibrary(thisItem.Filepath, thisItem.Filename, thisItem.Title, thisItem.Duration, thisItem.Artist, thisItem.Album, thisItem.Filetype,thisItem.Position );
        }


        public void addToLibrary(String fileLocation, String fileName, String title, long duration, String Artist, String Album)
        {
            string fileType = "";
            string[] splitOnPeriod;
            if (!string.IsNullOrEmpty(fileLocation) && fileName.Contains("."))
            {
                splitOnPeriod = fileLocation.Split('.');
                fileType = (splitOnPeriod[splitOnPeriod.Length - 1]);
            }

            addToLibrary(fileLocation,fileName,title,duration,Artist, Album, fileType, 0);
        }

        public void addToLibrary(String fileLocation, String fileName, String title, long duration, String Artist, String Album, string fileType, double Position)
        {

            sqlCommand.CommandText = "INSERT INTO library (FileName, Path, FileType, Title, duration, Artist, Album, Position) VALUES (?, ?, ?, ?, ?, ?, ?, ?);";
            sqlCommand.Parameters.Add("@FileName", DbType.String).Value = fileName;
            sqlCommand.Parameters.Add("@Path",     DbType.String).Value = fileLocation;
            sqlCommand.Parameters.Add("@FileType", DbType.String).Value = fileType;
            sqlCommand.Parameters.Add("@Title",    DbType.String).Value = title;
            sqlCommand.Parameters.Add("@duration", DbType.Int64).Value  = duration;
            sqlCommand.Parameters.Add("@Artist",   DbType.String).Value = Artist;
            sqlCommand.Parameters.Add("@Album",    DbType.String).Value = Album;
            sqlCommand.Parameters.Add("@Position", DbType.Double).Value = Position;
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (SQLiteException sqle)
            {
                if (sqle.ErrorCode == 19)
                    ; //carry on
                else
                    throw sqle;
            }            
        }

        public void addToPlaylist(String Playlist, String id)
            {
                throw new NotImplementedException();
            }
            public void retrievePlaylistToDataGrid(System.Windows.Controls.DataGrid target)
            {
                sqlCommand.CommandText = "SELECT * FROM library";



                DataSet dataSet = new DataSet();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlCommand);

                adapter.Fill(dataSet);
                target.ItemsSource = dataSet.Tables[0].DefaultView;


            }

    }
}

