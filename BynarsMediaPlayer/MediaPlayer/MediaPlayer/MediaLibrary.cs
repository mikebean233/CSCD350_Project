using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaPlayer
{
    public  class MediaLibrary
    {
        private string _name;
        private HashSet<MediaItem> _library;
        private LinkedList<MediaItem> _reverseLibrary; 
        private MainController _mainController;
        private MediaItem _currentItem;
        private IEnumerator<MediaItem> _itemPointer;
        private bool _deletable ;
        
        public MediaLibrary(string name, MainController mainController)
        {
            _name = (string.IsNullOrEmpty(name)) ? "" : name;
            _mainController = mainController;
            _library = new HashSet<MediaItem>();
            _itemPointer = _library.GetEnumerator();
            _reverseLibrary = new LinkedList<MediaItem>();
            _currentItem = new MediaItem();
            _deletable = false;
        }

        public string Name
        {
            get { return _name; }
        }

        public bool Deletable
        {
            get { return _deletable; }
            set { _deletable = value; }
        }

        public int AddNewMediaItem(MediaItem item)
        {
            if (item != null && !_library.Contains(item))
            {
                _reverseLibrary.AddFirst(item.Clone());
                _library.Add(item.Clone());
            }
            if (_library.Count == 1)
            {
                _currentItem = item;
                _currentItem.IsPlaying = true;
            }

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
            _reverseLibrary.Remove(item);
            _library.Remove(item);

           ResetItemPointerToCurrent();

            return _library.Count;
        }
        
        public MediaItem GetCurrentMedia()
        {
            if (_currentItem.Filepath == "" && _library.Any())
            {
                if(_itemPointer != null)
                    _itemPointer.Dispose();
                _itemPointer = _library.GetEnumerator();
                _itemPointer.MoveNext();
                _currentItem = _itemPointer.Current;
            }
            
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

            if (_currentItem == null)
                _currentItem = items.ToList()[0];

            foreach (MediaItem thisItem in items)
            {
                if (!_library.Contains(thisItem))
                {
                    _reverseLibrary.AddFirst(thisItem.Clone());
                    _library.Add(thisItem.Clone());
                }
            }
            ResetItemPointerToCurrent();

            return _library.Count;
        }

        private void ResetItemPointerToCurrent()
        {
            _itemPointer.Dispose();
            _itemPointer = _library.GetEnumerator();
            while(!_itemPointer.MoveNext() && !_itemPointer.Current.Equals(_currentItem)) { }
        }

        public bool SetCurrentMedia(MediaItem newCurrent)
        {
            if (!_library.Any() || newCurrent == null || !_library.Contains(newCurrent))
                return false;

            if (_currentItem.Equals(newCurrent))
            {
                newCurrent.IsPlaying = true;
                return true;
            }
            else
                _currentItem.IsPlaying = false;

            _itemPointer.Reset();
            _itemPointer.MoveNext();
            while (!_itemPointer.Current.Equals(newCurrent)) { _itemPointer.MoveNext();}
            _currentItem = _itemPointer.Current;
            _currentItem.IsPlaying = true;
            return true;
        }

        public MediaItem GetPreviousSong()
        {
            if (_currentItem != null)
                _currentItem.IsPlaying = false;

            if (_reverseLibrary.Any())
            {
                if (_currentItem == null)
                {
                    _currentItem = _reverseLibrary.First.Value;
                }
                else if (_reverseLibrary.Last.Value.Equals(_currentItem))
                {
                    _currentItem = _reverseLibrary.First.Value;
                }
                else
                {
                    LinkedListNode<MediaItem> thisNode = _reverseLibrary.Find(_currentItem);
                    _currentItem = thisNode.Next.Value;
                }
            }
            else
            {// Using a null object, this should never be necessary...
                _currentItem = new MediaItem();
            }

            _currentItem.IsPlaying = true;
            ResetItemPointerToCurrent();
            return _currentItem;
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

        public override bool Equals(object obj)
        {
            if (obj == null || (obj.GetType() != this.GetType()) || this._name == "")
                return false;
            MediaLibrary that = (MediaLibrary) obj;
            if (that._name == "")
                return false;

            return _name.Equals(that.Name);
        }
        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }
 
        public override string ToString()
        {
            return _name + ": " + _library.Count;
        }
    }
}
