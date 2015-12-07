using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace MediaPlayer
{
    public class PlaylistViewRow
    {
        private string _name ;
        private bool _deletable;

        public string Name
        {
            get { return _name; }
            set { if (value != null) _name = value; }
        }

        public bool Deletable
        {
            get { return _deletable; }
            set { _deletable = value; }
        }

        public PlaylistViewRow()
        {
            Name = "";
            Deletable = false;
        }
    }
}