using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentMPC.ViewModels.Playback
{
    /// <summary>
    /// For a PlaybackViewModel, this enum is used to discern the type of the view hosting it.
    /// The int values match the width of the album art in said views, to be used when decoding the art bitmaps.
    /// </summary>
    public enum VisualizationType
    {
        None = -1,
        NowPlayingBar = 70,
        FullScreenPlayback = 720,
        OverlayPlayback = 500
    }

    public static class EnumExtensions
    {
        public static bool IsOneOf(this VisualizationType enumeration, params VisualizationType[] enums)
        {
            return enums.Contains(enumeration);
        }
    }

}
