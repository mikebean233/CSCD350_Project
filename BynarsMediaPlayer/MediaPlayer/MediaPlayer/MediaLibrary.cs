using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    class MediaLibrary
    {
        private string _name;
        private HashSet<MediaItem> _library;
        private MainController _mainController;
        private MediaItem _currentItem;
        private IEnumerator<MediaItem> _itemPointer; 

        public MediaLibrary(string name, MainController mainController)
        {
            _name = name;
            _mainController = mainController;
            _library = new HashSet<MediaItem>();
            _itemPointer = _library.GetEnumerator();
        }

        public string Name
        {
            get { return _name; }
        }

        public int AddNewMediaItem(MediaItem item)
        {
            if (item != null)
            {
                _library.Add(item);
            }
            if (_library.Count == 1)
                _currentItem = item;

            ResetItemPointerToCurrent();

            return _library.Count;
        }

        public int RemoveItem(MediaItem item)
        {
            if (item == null || !_library.Any())
                return _library.Count;

            if (_library.Count == 1 && _library.Contains(item))
            {
                _currentItem.IsPlaying = false;
                _currentItem = null;
                _itemPointer.Dispose();
                _library.Remove(item);
                return 0;
            }
            if (_currentItem.Equals(item))
                GetNextSong();
            _library.Remove(item);

           ResetItemPointerToCurrent();

            return _library.Count;
        }

        public MediaItem GetCurrentSong()
        {
            return _currentItem;
        }

        public List<MediaItem> GetMedia()
        {
            return _library.ToList();
        } 

        public bool Any() { return _library.Any();}
        public int Count{get{return _library.Count;} }

        public int AddRange(ICollection<MediaItem> items)
        {
            if (items == null || !items.Any())
                return _library.Count;

            foreach (MediaItem thisItem in items)
                _library.Add(thisItem);
            ResetItemPointerToCurrent();

            return _library.Count;
        }

        private void ResetItemPointerToCurrent()
        {
            _itemPointer.Dispose();
            _itemPointer = _library.GetEnumerator();
            while(!_itemPointer.MoveNext() && !_itemPointer.Current.Equals(_currentItem)) { }
        }

        public MediaItem GetNextSong()
        {
            if (!_library.Any() || _mainController == null)
                return null;
            if (_currentItem != null)
                _currentItem.IsPlaying = false;
            switch (_mainController.PlayMode)
            {
                case PlayModeEnum.Consecutive:
                    if (!_itemPointer.MoveNext())
                    {
                        
                        _itemPointer.Dispose();
                        _itemPointer = _library.GetEnumerator();
                        _itemPointer.MoveNext();
                    }
                    _currentItem = _itemPointer.Current;
                    break;
                case PlayModeEnum.Repeat:
                    // do nothing
                    break;
                case PlayModeEnum.Shuffle:
                    int randomIndex = Utilities.GetRandomInt(0, _library.Count - 1);
                    _itemPointer.Dispose();
                    _itemPointer = _library.GetEnumerator();

                    while ((randomIndex--) != 0){ _itemPointer.MoveNext();}
                    _currentItem = _itemPointer.Current;
                break;
            }
            if(_currentItem != null)
                _currentItem.IsPlaying = true;
            return _currentItem;
        }

    }
}
