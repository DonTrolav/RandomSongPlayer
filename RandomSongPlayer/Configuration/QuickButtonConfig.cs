using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomSongPlayer.Configuration
{
    internal class QuickButtonConfig
    {
        public virtual ShowMode ShowMode { get; set; } = ShowMode.OnRandomPack;
        public virtual float PositionX { get; set; } = 11;
        public virtual float PositionY { get; set; } = -13;
        public virtual float Width { get; set; } = 10;
        public virtual float Height { get; set; } = 8;
    }

    internal enum ShowMode
    {
        Never,
        OnRandomPack,
        Always
    }
}
