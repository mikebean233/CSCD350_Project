using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.Tests
{
    [TestClass()]
    public class DatabaseControllerTests
    {
        public DatabaseController DBCon;

        [TestMethod()]
        public void DatabaseControllerTest()
        {
            DBCon = new DatabaseController();
        }

        [TestMethod()]
        public void GetMediaItemsFromDatabaseTest()
        {
            DBCon = new DatabaseController();
            List<MediaItem> items = DBCon.GetMediaItemsFromDatabase();
            Console.Out.WriteLine(items.Count);
            Assert.AreEqual(0,items.Count );
        }

        [TestMethod()]
        public void AddMediaItemsToDatabaseTest()
        {
            DBCon = new DatabaseController();
            List<MediaItem> items = new List<MediaItem>();
            items.Add(new MediaItem(Environment.CurrentDirectory + "\\test.mp3"));
            DBCon.AddMediaItemsToDatabase(items);

            items = DBCon.GetMediaItemsFromDatabase();

            Assert.AreEqual(1, items.Count);

        }

        [TestMethod()]
        public void AddMediaItemsToDatabaseTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void addToLibraryTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void addToLibraryTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void addToPlayListTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void addToPlayListTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void searchTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void searchTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void retrievePlaylistToDataGridTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void retrievePlaylistToDataGridTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void removeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void removeTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void addPlayListTest()
        {
            DBCon = new DatabaseController();
            try
            {
                DBCon.addPlayList("");
                DBCon.addPlayList("123");
                DBCon.addPlayList("new play list");
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void getStringTest()
        {
            Assert.Fail();
        }
    }
}