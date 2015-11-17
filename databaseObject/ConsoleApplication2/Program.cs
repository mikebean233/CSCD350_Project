using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace media_database
{
    class media_database
    {
        static void Main(string[] args)
        {
            new media_database();
        }

        SQLiteConnection sqlConnection;
        SQLiteCommand sqlComand;

        public media_database()
        {
            
            
            if (!File.Exists("media.DB"))
            {
                SQLiteConnection.CreateFile("media.sqlite");
            }

            sqlConnection = new SQLiteConnection("Data Source = media.DB;Version = 3");
            sqlConnection.Open();
            String sql = "CREATE TABLE IF NOT EXISTs library (Name VARCHAR, Path VARCHAR UNIQUE, FileType VARCHAR, Title VARCHAR, Durration VARCHAR, Artist VARCHAR, Album VARCHAR) ";
            sqlComand = new SQLiteCommand(sql, sqlConnection);
            sqlComand.ExecuteNonQuery();
            

            addToLibrary("c:\\text.txt", "text.txt", "text","1", "travis", "travis's greatest hits");
            libraryToString();

            Console.In.Read();
        }

        private void libraryToString()
        {
            sqlComand.CommandText = "SELECT * FROM library";
            
            SQLiteDataReader reader = sqlComand.ExecuteReader();

            while (reader.Read())
                Console.WriteLine(reader["Name"] + " " +reader["Path"]  + " " + reader["FileType"] + " " + reader["Title"] + " " + reader["Durration"] + " " + reader["Artist"] + " " + reader["Album"]);
        }

        public void addToLibrary(String fileLocation, String fileName, String title, String durration, String Artist, String Album)
        {


            
            


            sqlComand.CommandText = "INSERT INTO library (Name, Path, FileType, Title, Durration, Artist, Album) VALUES (?, ?, ?, ?, ?, ?, ?);";
            sqlComand.Parameters.Add("@Name", DbType.String).Value = fileName;
            sqlComand.Parameters.Add("@Path", DbType.String).Value = fileLocation;
            sqlComand.Parameters.Add("@FileType", DbType.String).Value = fileName.Substring(fileName.IndexOf("."),4);
            sqlComand.Parameters.Add("@Title", DbType.String).Value = title;
            sqlComand.Parameters.Add("@Durration", DbType.String).Value = durration;
            sqlComand.Parameters.Add("@Artist", DbType.String).Value = Artist;
            sqlComand.Parameters.Add("@Album", DbType.String).Value = Album;
            try
            {
                sqlComand.ExecuteNonQuery();
            }
            catch(SQLiteException sqle)
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
