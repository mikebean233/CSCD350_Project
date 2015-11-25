using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    class MediaItem
    {
        public string Album { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public string Artist { get; set; }
        // public string Id { get; set; }
        public int Duration { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public string Filepath { get; set; }
        public int Position { get; set; }

        public MediaItem(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new Exception("An attempt was made to create a media item without a path");
            Filepath = path;
        }

        public override int GetHashCode()
        {
            if (String.IsNullOrEmpty(Filepath))
                return 0;

            return Filepath.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(string.IsNullOrEmpty(Filepath) || (obj.GetType() != this.GetType()))
                return false;

            return Filepath.Equals(((MediaItem)obj).Filepath);
        }

        public override string ToString()
        {
            return (string.IsNullOrEmpty(Filepath)) ? "" : Filepath.ToString();
        }
    }
}
