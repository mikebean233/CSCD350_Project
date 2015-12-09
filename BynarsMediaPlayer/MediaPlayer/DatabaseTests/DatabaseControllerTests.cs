using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MediaPlayer.Tests
{
    [TestClass()]
    public class DatabaseControllerTests
    {
        public DatabaseController DBCon;

        [ClassInitialize()]
        public static void Initializeclass(TestContext t)
        {
            File.Delete(Environment.CurrentDirectory + "\\media.DB");

        }

        [TestInitialize()]
        public void Initialize()
        {
            DBCon = new DatabaseController();
        }

        [TestCleanup()]
        public void Cleanup()
        {
            //MessageBox.Show("TestMethodCleanup");
        }



        [TestMethod()]
        public void DatabaseControllerTest()
        {
            if (DBCon == null)
                Assert.Fail();
        }

        [TestMethod()]
        public void GetMediaItemsFromDatabaseTest()
        {
            DBCon = new DatabaseController();
            List<MediaItem> items = DBCon.GetMediaItemsFromDatabase();
            Console.Out.WriteLine(items.Count);
            Assert.AreEqual(0, items.Count);
        }

        [TestMethod()]
        public void AddMediaItemsToDatabaseTest()
        {
            DBCon = new DatabaseController();
            List<MediaItem> items = new List<MediaItem>();
            items.Add(Utilities.BuildMediaItemFromPath(Environment.CurrentDirectory + "\\test.mp3"));

            DBCon.AddMediaItemsToDatabase(items);

            items = DBCon.GetMediaItemsFromDatabase();

            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(Environment.CurrentDirectory + "\\test.mp3", items[0].Filepath);

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
                DBCon.addPlayList("testPlaylist");
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void AddMediaItemsToDatabaseTest1()
        {
            DBCon = new DatabaseController();
            List<MediaItem> items = new List<MediaItem>();
            items.Add(Utilities.BuildMediaItemFromPath(Environment.CurrentDirectory + "\\test.mp3"));
            DBCon.AddMediaItemsToDatabase("testPlaylist", items);
            items = DBCon.retrievePlaylist("testPlaylist");

            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(Environment.CurrentDirectory + "\\test.mp3", items[0].Filepath);
            Assert.AreEqual("yup", items[0].Genre);
        }





        [TestMethod()]
        public void searchTest()
        {
            List<TagType> t = new List<TagType>();
            t.Add(TagType.Album);
            t.Add(TagType.Artist);
            t.Add(TagType.Genre);
            t.Add(TagType.Title);
            List<MediaItem> items = new List<MediaItem>();
            items.Add(Utilities.BuildMediaItemFromPath(Environment.CurrentDirectory + "\\test.mp3"));
            DBCon.AddMediaItemsToDatabase(items);

            List<MediaItem> m = DBCon.search("not in", t);
            Assert.AreEqual(0, m.Count);

            m = DBCon.search("yup", t);
            Assert.AreEqual(1, m.Count);
            Assert.AreEqual("yup", m[0].Genre.ToString());


        }

        [TestMethod()]
        public void searchTest1()
        {
            List<TagType> t = new List<TagType>();
            t.Add(TagType.Album);
            t.Add(TagType.Artist);
            t.Add(TagType.Genre);
            t.Add(TagType.Title);
            List<MediaItem> items = new List<MediaItem>();
            items.Add(Utilities.BuildMediaItemFromPath(Environment.CurrentDirectory + "\\test.mp3"));
            DBCon.addPlayList("searchtest1");
            DBCon.AddMediaItemsToDatabase("searchtest1", items);

            List<MediaItem> m = DBCon.search("searchtest1", "not in", t);
            Assert.AreEqual(0, m.Count);

            m = DBCon.search("searchtest1", "yup", t);
            Assert.AreEqual(1, m.Count);
            Assert.AreEqual("yup", m[0].Genre.ToString());
        }

              



        [TestMethod()]
        public void getStringTest()
        {
            // Wrap an already existing instance
            PrivateObject accessor = new PrivateObject(new DatabaseController());

            

            // Call a private method
            Object Obj = accessor.Invoke("getString", TagType.Album);
            Assert.AreEqual("Album", (string)Obj);


        }

        [TestMethod()]
        public void removeTest()
        {
            List<MediaItem> items = new List<MediaItem>();
            items.Add(Utilities.BuildMediaItemFromPath(Environment.CurrentDirectory + "\\test.mp3"));
            DBCon.addPlayList("removetest0");
            DBCon.AddMediaItemsToDatabase("removetest0", items);

            DBCon.remove("removetest0", items);
            items = DBCon.retrievePlaylist("removetest0");
            Assert.AreEqual(0, items.Count);
        }

        [TestMethod()]
        public void removeTest1()
        {
            List<MediaItem> items = new List<MediaItem>();
            items.Add(Utilities.BuildMediaItemFromPath(Environment.CurrentDirectory + "\\test.mp3"));
            DBCon.remove(items);
            items = DBCon.retrievePlaylist();
            Assert.AreEqual(0, items.Count);
        }


        [TestMethod()]
        public void initTest()
        {
            // Wrap an already existing instance
            PrivateObject accessor = new PrivateObject(new DatabaseController());


            int member = (int)accessor.GetField("_playlistID");
            
            Assert.AreNotEqual(1, member);
        }

    }



}