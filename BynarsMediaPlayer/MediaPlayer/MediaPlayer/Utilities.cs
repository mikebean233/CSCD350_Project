using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.PerformanceData;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    class Utilities
    {
        private static Random _random;

        public static int GetRandomInt(int min, int max)
        {
            if (_random == null)
                _random = new Random();

            return _random.Next(min, max);
        }
        public static MediaItem BuildMediaItemFromPath(string path)
        {
            MediaItem thisItem = new MediaItem(path);
            
            Dictionary<string, string> tags = TagManager.GetMediaTags(path);

            thisItem.Album = tags["album"];
            thisItem.Title = tags["title"];
            thisItem.Year = tags["year"];
            //thisItem.Id = tags["id"];
            thisItem.Artist = tags["artist"];
            thisItem.Duration = 0;
            try { thisItem.Duration = Int32.Parse(tags["duration"]);}catch(Exception e) { }
            thisItem.Filename = tags["filename"];
            thisItem.Filetype = tags["filetype"];
            thisItem.Position = 0;

            return thisItem;
        }

        public static string BuildStringFromTimeSpan(TimeSpan input)
        {
            if (input == null)
                return "0:00:00";
            string seconds = ((int)input.TotalSeconds % 60).ToString();
            string minutes = ((int)input.TotalMinutes % 60).ToString();
            string hours = ((int)input.TotalHours % 24).ToString();

            if (seconds.Length == 1) seconds = "0" + seconds;
            if (minutes.Length == 1) minutes = "0" + minutes;

            return hours + ":" + minutes + ":" + seconds;
        }

        public static TimeSpan BuildTimspanFromPerportion(double perportion, TimeSpan totalTime)
        {
            if(perportion == 0.0 || totalTime == null || totalTime.TotalMilliseconds == 0)
                return new TimeSpan(0);
            if (perportion == 1.0)
                return new TimeSpan(totalTime.Ticks);
            
            int milliseconds = (int)(totalTime.TotalMilliseconds * perportion) % 1000;
            int seconds = (int)(totalTime.TotalSeconds * perportion) % 60;
            int minutes = (int)(totalTime.TotalMinutes * perportion) % 60;
            int hours = (int)(totalTime.TotalHours * perportion) % 24;
            int days = 0;
            return new TimeSpan(days, hours, minutes, seconds, milliseconds);
        }
        public static bool SerializeObjectToJson<T>(string outputFileName, T obj)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write, FileShare.None);

                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, obj);
                stream.Close();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static T DeserializeObjectFromJson<T>(string inputFileName)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read, FileShare.None);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            T outputObject = (T) serializer.ReadObject(stream);
            stream.Close();
            return outputObject;
        }

    }
}
