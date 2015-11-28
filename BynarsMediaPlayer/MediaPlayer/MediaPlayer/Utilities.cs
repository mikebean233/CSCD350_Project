using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Linq;
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

    }
}
