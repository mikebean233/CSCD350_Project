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
    class DatabaseControllerStub
    {
       SQLiteConnection sqlConnection;
            SQLiteCommand sqlCommand;

            public DatabaseControllerStub()
            {
                if (!File.Exists("media.DB"))
                {
                    SQLiteConnection.CreateFile("media.sqlite");
                }

                sqlConnection = new SQLiteConnection("Data Source = media.DB;Version = 3");
                sqlConnection.Open();
                String sql = "CREATE TABLE IF NOT EXISTs library (Name VARCHAR, Path VARCHAR UNIQUE, FileType VARCHAR, Title VARCHAR, Duration VARCHAR, Artist VARCHAR, Album VARCHAR) ";
                sqlCommand = new SQLiteCommand(sql, sqlConnection);
                sqlCommand.ExecuteNonQuery();


                addToLibrary("c:\\text.txt", "text.txt", "text", "1", "travis", "travis's greatest hits");
                //libraryToString();

                //Console.In.Read();
            }

            private void libraryToString()
            {
                sqlCommand.CommandText = "SELECT * FROM library";

                SQLiteDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                    Console.WriteLine(reader["Name"] + " " + reader["Path"] + " " + reader["FileType"] + " " + reader["Title"] + " " + reader["Durration"] + " " + reader["Artist"] + " " + reader["Album"]);
            }

            public void addToLibrary(String fileLocation, String fileName, String title, String duration, String Artist, String Album)
            {
                try
                {
                    sqlCommand.CommandText = "INSERT INTO library (Name, Path, FileType, Title, Duration, Artist, Album) VALUES (?, ?, ?, ?, ?, ?, ?);";
                    sqlCommand.Parameters.Add("@Name", DbType.String).Value = fileName;
                    sqlCommand.Parameters.Add("@Path", DbType.String).Value = fileLocation;
                    sqlCommand.Parameters.Add("@FileType", DbType.String).Value = fileName.Substring(fileName.IndexOf("."), 4);
                    sqlCommand.Parameters.Add("@Title", DbType.String).Value = title;
                    sqlCommand.Parameters.Add("@Durration", DbType.String).Value = duration;
                    sqlCommand.Parameters.Add("@Artist", DbType.String).Value = Artist;
                    sqlCommand.Parameters.Add("@Album", DbType.String).Value = Album;
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
                throw new NotImplementedException();
            }

        }
}

