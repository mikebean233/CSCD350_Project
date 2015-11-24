using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Id3Lib;
using Mp3Lib;
namespace MediaPlayer
{
    class TagManager
    {
        private static Dictionary<string, string> _knownTags;
        private static bool _initialized = false;
        public static void Initialize()
        {
            if (_knownTags == null)
            {
                _knownTags = new Dictionary<string, string>();
                _knownTags.Add("TALB","album");
                _knownTags.Add("TIT2","title");
                _knownTags.Add("TYER","year");
                _knownTags.Add("UFID","id");
                _knownTags.Add("TPE2","artist");
                _knownTags.Add("TLEN","duration");
                _knownTags.Add("TOFN","filename");
                _knownTags.Add("TFLT", "filetype");
            }
            _initialized = true;
        }
        
         
        public static Dictionary<string, string> GetMediaTags(string mediaPath)
        {
            if(!_initialized)
                Initialize();
            Dictionary<string, string> ID3Tags = new Dictionary<string, string>();
            foreach (string thisValue in _knownTags.Values)
                ID3Tags[thisValue] = "unknown " + thisValue;
            
            if (!string.IsNullOrEmpty(mediaPath))
            {
                //determine the type of the file
                if (!mediaPath.Contains("."))
                    return ID3Tags;
                string[] splitByPeriod = mediaPath.Split('.');
                string extention = splitByPeriod[splitByPeriod.Length - 1];

                try
                {
                    switch (extention.ToLower())
                    {
                        case "mp3":
                            Mp3Lib.Mp3File mp3File = new Mp3File(mediaPath);
                            TagModel tagModel = mp3File.TagModel;
                            foreach(Id3Lib.Frames.FrameText thisFrame in tagModel)
                                if(_knownTags.ContainsKey(thisFrame.FrameId))
                                    ID3Tags[_knownTags[thisFrame.FrameId]] = thisFrame.Text;

                            break;
                        case "wma":
                            break;
                        default:
                            return ID3Tags;
                    }
                }
                catch (Exception e)
                {
                    return ID3Tags;
                }
            }// end switch
            return ID3Tags;
        }
         
    }
}
