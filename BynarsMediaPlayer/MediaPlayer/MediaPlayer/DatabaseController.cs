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
    public class DatabaseController
    {
        private SQLiteConnection sqlConnection;
        private int _playlistID;
        
        public DatabaseController()
        {
            if (!File.Exists("media.DB"))
            {
                SQLiteConnection.CreateFile("media.sqlite");
            }
            
            sqlConnection = new SQLiteConnection("Data Source = media.DB;Version = 3");
            sqlConnection.Open();
            String sql = "CREATE TABLE IF NOT EXISTs library (FileName VARCHAR, Path VARCHAR UNIQUE, FileType VARCHAR, Title VARCHAR, Duration INTEGER, Artist VARCHAR, Album VARCHAR, Genre VARCHAR, Position REAL) ";
            using (SQLiteCommand sqlCommand = new SQLiteCommand(sql, sqlConnection))
            {
                sqlCommand.ExecuteNonQuery();

                sql = "CREATE TABLE IF NOT EXISTS Playlists (playlist VARCHAR UNIQUE, tableName VARCHAR UNIQUE)";
                sqlCommand.CommandText = sql;
                sqlCommand.ExecuteNonQuery();
            }
            initPlayListID();
            Console.Out.WriteLine("done initalizing database");
            }

        private void initPlayListID()
        {
            try
            {
                using (SQLiteCommand sqlCommand = new SQLiteCommand(sqlConnection))
                {
                    string sql = "Select tableName From Playlists ORDER BY TableName DESC LIMIT 1";
                    sqlCommand.CommandText = sql;
                    SQLiteDataReader sqlReader = sqlCommand.ExecuteReader();


                    _playlistID = Int32.Parse(sqlReader.GetString(0));
                    sqlReader.Close();
                    sqlReader.Dispose();
                }
            }
            catch
            {
                _playlistID = 1;
                return;
            }


        }

        public List<MediaItem> GetMediaItemsFromDatabase()
        {
            List<MediaItem> items = new List<MediaItem>();
            using (SQLiteCommand sqlCommand = new SQLiteCommand(sqlConnection))
            {
                Console.Out.WriteLine("getMediaItems");
                
                sqlCommand.CommandText = "SELECT * FROM library";
                using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
                {
                    try
                    {
                        items = readerToList(reader);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Error in GetMediaItemsFromDataBase: " + e);
                    }
                }
            }
            return items;
        }

        public void AddMediaItemsToDatabase(ICollection<MediaItem> items)
        {
            if (items == null || !items.Any())
                return;
            foreach(MediaItem thisItem in items)
                addToLibrary(thisItem.Filepath, thisItem.Filename, thisItem.Title, thisItem.Duration, thisItem.Artist, thisItem.Album, thisItem.Genre, thisItem.Filetype,thisItem.Position );
        }
        public void AddMediaItemsToDatabase(string playlist, ICollection<MediaItem> items)
        {
            if (items == null || !items.Any())
                return;
            string tableName = getTableName(playlist);//this is faster as it doesn't require multiple calls to figure out the tableName.
            foreach (MediaItem thisItem in items)
                addToLibraryUNSAFE(tableName, thisItem.Filepath, thisItem.Filename, thisItem.Title, thisItem.Duration, thisItem.Artist, thisItem.Album, thisItem.Genre, thisItem.Filetype, thisItem.Position);
        }

        #region addToLibrary
        public void addToLibrary(                         String fileLocation, String fileName, String title, long duration, String Artist, String Album, String Genre)
        {
            string fileType = "";
            string[] splitOnPeriod;
            if (!string.IsNullOrEmpty(fileLocation) && fileName.Contains("."))
            {
                splitOnPeriod = fileLocation.Split('.');
                fileType = (splitOnPeriod[splitOnPeriod.Length - 1]);
            }

            addToLibraryUNSAFE("library",fileLocation,fileName,title,duration,Artist, Album, Genre, fileType, 0);
        }
        public void addToLibrary(                         String fileLocation, String fileName, String title, long duration, String Artist, String Album, String Genre, string fileType, double Position)
        {
            addToLibraryUNSAFE("library", fileLocation, fileName, title, duration, Artist, Album, Genre, fileType, 0);
        }
        public void addToPlayList(      string playlist,  String fileLocation, String fileName, String title, long duration, String Artist, String Album, String Genre)
        {
            string fileType = "";
            string tableName;
            string[] splitOnPeriod;
            if (!string.IsNullOrEmpty(fileLocation) && fileName.Contains("."))
            {
                splitOnPeriod = fileLocation.Split('.');
                fileType = (splitOnPeriod[splitOnPeriod.Length - 1]);
            }
            tableName = getTableName(playlist);
            addToLibraryUNSAFE(tableName, fileLocation, fileName, title, duration, Artist, Album, Genre, fileType, 0); 
        }
        public void addToPlayList(      string playlist,  String fileLocation, String fileName, String title, long duration, String Artist, String Album, String Genre, string fileType, double Position)
        {
            string tableName = getTableName(playlist);
            addToLibraryUNSAFE(tableName, fileLocation, fileName, title, duration, Artist, Album, Genre, fileType, 0);
        }
        private void addToLibraryUNSAFE(string tableName, String fileLocation, String fileName, String title, long duration, String Artist, String Album, String Genre, string fileType, double Position)
        {
            using (SQLiteCommand sqlCommand = new SQLiteCommand(sqlConnection))
            {
                sqlCommand.CommandText = "INSERT INTO "+ tableName +" (FileName, Path, FileType, Title, duration, Artist, Album, Genre, Position) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);";
                sqlCommand.Parameters.Add("@FileName", DbType.String).Value = fileName;
                sqlCommand.Parameters.Add("@Path",     DbType.String).Value = fileLocation;
                sqlCommand.Parameters.Add("@FileType", DbType.String).Value = fileType;
                sqlCommand.Parameters.Add("@Title",    DbType.String).Value = title;
                sqlCommand.Parameters.Add("@duration", DbType.Int64).Value  = duration;
                sqlCommand.Parameters.Add("@Genre",    DbType.String).Value = Genre;
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
        }
        #endregion
        #region search
        public List<MediaItem> search(                          string toSearch, TagType searchIn)
        {
            return searchUNSAFE("library", toSearch, searchIn);
        }
        public List<MediaItem> search(         string playlist, string toSearch, TagType searchIn)
        {
            string tableName = getTableName(playlist);
            return searchUNSAFE(tableName, toSearch, searchIn);
        }
        private List<MediaItem> searchUNSAFE( string tableName, string toSearch, TagType searchIn)
        {
            List<MediaItem> items = new List<MediaItem>();
            using (SQLiteCommand sqlCommand = new SQLiteCommand(sqlConnection))
            {
                sqlCommand.CommandText = "SELECT " + tableName + " FROM library WHERE @TagType LIKE @Search";
                sqlCommand.Parameters.Add("@TagType", DbType.String).Value = getString(searchIn);
                sqlCommand.Parameters.Add("@Seach", DbType.String).Value = "%" + toSearch + "%";

                using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
                {
                    try
                    {
                        items = readerToList(reader);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error in search: " + e);
                    }
                }//end using dataReader


            }
            return items;
        }
        #endregion
        #region retrievePlaylistToDataGrid
        /// <summary>
        /// if no argement is specified this functions as getMediaItemsFromDatabase()
        /// </summary>
        /// <returns></returns>
        public List<MediaItem> retrievePlaylist()
        {
            return retrievePlaylistUNSAFE("library");
        }
        public List<MediaItem> retrievePlaylist(       string playList)
        {
            string tableName = getTableName(playList);
            return retrievePlaylistUNSAFE(tableName);
        }
        private List<MediaItem> retrievePlaylistUNSAFE(string tableName)
        {
            List<MediaItem> items = new List<MediaItem>();
            using (SQLiteCommand sqlCommand = new SQLiteCommand(sqlConnection))
            {
                sqlCommand.CommandText = "SELECT " + tableName + " FROM library";
                using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
                {
                    try
                    {
                        items = readerToList(reader);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error in retrievePlaylistTo: " + e);
                    }
                }
            }
            return items;
            
        }
        #endregion
        #region remove
        public void remove(                         List<MediaItem> toRemove)
        {
            removeUnsafe("library", toRemove);
        }
        public void remove(       string playList,  List<MediaItem> toRemove)
        {
            removeUnsafe(getTableName(playList), toRemove);
        }
        private void removeUnsafe(string tableName, List<MediaItem> toRemove)
        {
            using (SQLiteCommand sqlCommand = new SQLiteCommand(sqlConnection))
            {
                foreach (MediaItem m in toRemove)
                {
                    sqlCommand.CommandText = "DELETE FROM " + tableName + " WHERE Path = @path";
                    sqlCommand.Parameters.Add("@path", DbType.String).Value = m.Filepath;
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
        #endregion
        #region addPlaylist
        /// <summary>
        /// add a playlist to the playlist table and add the playlists table to the database.
        /// </summary>
        /// <param name="playlist"></param>
        public void addPlayList(string playlist)
        {
            string playlistID = getTableName(playlist);
            if (playlistID == null)
            {
                playlistID = "playlist" + _playlistID;
                _playlistID++;
            }
            else
                throw new DuplicateNameException("Playlist '" + playlist + "' already exists.");

            using (SQLiteCommand sqlCommand = new SQLiteCommand(sqlConnection))
            {
                string sql = "INSERT INTO Playlist (playlist, tableName) VALUES (?,?)";
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add("@playlist", DbType.String).Value = playlist;
                sqlCommand.Parameters.Add("@tableName", DbType.String).Value = playlistID;
                sqlCommand.ExecuteNonQuery();

                sql = "CREATE TABLE " + playlistID + " (FileName VARCHAR, Path VARCHAR UNIQUE, FileType VARCHAR, Title VARCHAR, Duration INTEGER, Artist VARCHAR, Album VARCHAR, Genre VARCHAR, Position REAL)";
                sqlCommand.CommandText = sql;
                sqlCommand.ExecuteNonQuery();
            }

        }
        public void addPlaylist(string playlist, List<MediaItem> contents)
        {
            addPlayList(playlist);
            AddMediaItemsToDatabase(playlist, contents);
        }
        #endregion
        public void removePlaylist(string playlist)
        {
            string tableName = getTableName(playlist);

            using (SQLiteCommand sqlCommand = new SQLiteCommand(sqlConnection))
            {
                string sql = "DELECT FROM Playlist WHERE playlist = @playlist";
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add("@playlist", DbType.String).Value = playlist;
                sqlCommand.ExecuteNonQuery();

                sql = "DROP TABLE " + tableName;
                sqlCommand.CommandText = sql;
                sqlCommand.ExecuteNonQuery();
            }
        }
        public List<string> getPlaylists()
        {
            List<string> items = new List<string>();
            using (SQLiteCommand sqlCommand = new SQLiteCommand(sqlConnection))
            {
                Console.Out.WriteLine("getPlaylists");

                sqlCommand.CommandText = "SELECT playlist FROM Playlists";
                using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            items.Add((string)reader["Playlist"]);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error in GetPlaylists: " + e);
                        }
                    }// end while
                }
            }
            
            return items;
        }

        #region helpers
        private string getString(TagType toConvert)
        {
            switch (toConvert)
            {
                case TagType.Album:
                    return "Album";
                case TagType.Artist:
                    return "Artist";
                case TagType.Genre:
                    return "Genre";
                case TagType.Title:
                    return "Title";
            }
            return null;
        }

        private List<MediaItem> readerToList(SQLiteDataReader reader)
        {
            List<MediaItem> items = new List<MediaItem>();
            while (reader.Read())
            {
                
                MediaItem thisItem = new MediaItem((string)reader["Path"])
                {
                    Album = (string)reader["Album"],
                    Artist = (string)reader["Artist"],
                    Duration = (long)reader["Duration"],
                    Filename = (string)reader["FileName"],
                    Filetype = (string)reader["FileType"],
                    Position = (double)reader["Position"],
                    Title = (string)reader["Title"]
                };
                items.Add(thisItem);
               
            }// end while

            return items;
        }

        private string getTableName(string playListName)
        {
            string tableName;

            using (SQLiteCommand sqlCommand = new SQLiteCommand(sqlConnection))
            {
                sqlCommand.CommandText = "SELECT tableName FROM Playlists WHERE playlist = @playlist";
                sqlCommand.Parameters.Add("@playlist", DbType.String).Value = playListName;
                SQLiteDataReader sqlReader = sqlCommand.ExecuteReader();
                tableName = sqlReader.GetString(0);
                sqlReader.Close();

            }
            return tableName;


        }
        #endregion

    }
}

