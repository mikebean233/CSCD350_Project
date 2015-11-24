using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Data;

namespace MediaPlayer
{
    class DatabaseController
    {
       SQLiteConnection sqlConnection;
            SQLiteCommand sqlCommand;

            public DatabaseController()
            {
                if (!File.Exists("media.DB"))
                {
                    SQLiteConnection.CreateFile("media.sqlite");
                }

                sqlConnection = new SQLiteConnection("Data Source = media.DB;Version = 3");
                sqlConnection.Open();
                String sql = "CREATE TABLE IF NOT EXISTs library (Name VARCHAR, Path VARCHAR UNIQUE, FileType VARCHAR, Title VARCHAR, Duration VARCHAR, Artist VARCHAR, Album VARCHAR, Position VARCHAR) ";
                sqlCommand = new SQLiteCommand(sql, sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }

        private void libraryToString()
        {
            sqlCommand.CommandText = "SELECT * FROM library";

            SQLiteDataReader reader = sqlCommand.ExecuteReader();

            while (reader.Read())
                Console.WriteLine(reader["Name"] + " " + reader["Path"] + " " + reader["FileType"] + " " + reader["Title"] + " " + reader["Durration"] + " " + reader["Artist"] + " " + reader["Album"] + reader["Position"]);
        }

        public void addToLibrary(String fileLocation, String fileName, String title, int duration, String Artist, String Album)
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

        public void addToLibrary(String fileLocation, String fileName, String title, int duration, String Artist, String Album, string fileType, int Position)
        {

            sqlCommand.CommandText = "INSERT INTO library (Name, Path, FileType, Title, duration, Artist, Album, Position) VALUES (?, ?, ?, ?, ?, ?, ?, ?);";
            sqlCommand.Parameters.Add("@Name", DbType.String).Value = fileName;
            sqlCommand.Parameters.Add("@Path", DbType.String).Value = fileLocation;
            sqlCommand.Parameters.Add("@FileType", DbType.String).Value = fileType;
            sqlCommand.Parameters.Add("@Title", DbType.String).Value = title;
            sqlCommand.Parameters.Add("@duration", DbType.Int64).Value = duration;
            sqlCommand.Parameters.Add("@Artist", DbType.String).Value = Artist;
            sqlCommand.Parameters.Add("@Album", DbType.String).Value = Album;
            sqlCommand.Parameters.Add("@Position", DbType.Int64).Value = Position;
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

