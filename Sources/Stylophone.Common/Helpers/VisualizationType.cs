using System;
using System.Linq;

namespace Stylophone.Common.Helpers
{
    /// <summary>
    /// For a PlaybackViewModel, this enum is used to discern the type of the view hosting it.
    /// </summary>
    public enum VisualizationType
    {
        None,
        NowPlayingBar,
        FullScreenPlayback,
        OverlayPlayback
    }

    public static class EnumExtensions
    {
        public static bool IsOneOf(this VisualizationType enumeration, params VisualizationType[] enums)
        {
            return enums.Contains(enumeration);
        }
    }

}
