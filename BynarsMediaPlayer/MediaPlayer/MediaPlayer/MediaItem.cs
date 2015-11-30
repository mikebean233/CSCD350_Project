using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    [DataContract]
    public class MediaItem
    {
        [DataMember]
        public string Album { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Year { get; set; }
        [DataMember]
        public string Artist { get; set; }
        // public string Id { get; set; }
        [DataMember]
        public long Duration { get; set; }
        [DataMember]
        public string Filename { get; set; }
        [DataMember]
        public string Filetype { get; set; }
        [DataMember]
        public string Filepath { get; set; }
        [DataMember]
        public double Position { get; set; }
        [DataMember]
        public bool IsPlaying { get; set; }

        public MediaItem(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new Exception("An attempt was made to create a media item without a path");
            Filepath = path;
            IsPlaying = false;
            Position = 0.0;
            Duration = 0;
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
