using System;
using System.Collections.Generic;
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
            thisItem.Id = tags["id"];
            thisItem.Artist = tags["artist"];
            thisItem.Duration = tags["duration"];
            thisItem.Filename = tags["filename"];
            thisItem.Filetype = tags["filetype"];

            return thisItem;
        }
    }
}
