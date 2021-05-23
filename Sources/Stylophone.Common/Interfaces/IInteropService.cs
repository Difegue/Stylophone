using SkiaSharp;
using Stylophone.Common.ViewModels;
using System;
using System.Threading.Tasks;

namespace Stylophone.Common.Interfaces
{
    public enum Theme
    {
        Default = 0,
        Light = 1,
        Dark = 2
    }

    public enum PlaybackIcon
    {
        Play,  //"\uE768";
        Pause, //"\uE769";
        Repeat, //"\uE8EE";
        RepeatSingle, //"\uE8ED";
        VolumeMute, //"\uE74F";
        Volume25, //"\uE992";
        Volume50, //"\uE993";
        Volume75, // "\uE994";
        VolumeFull //"\uE767";
    }

    public interface IInteropService
    {
        SKColor GetAccentColor();
        Task<SKImage> GetPlaceholderImageAsync();
        Task UpdateOperatingSystemIntegrationsAsync(TrackViewModel currentTrack);
        Version GetAppVersion();
        Task SetThemeAsync(Theme param);

        string GetIcon(PlaybackIcon volumeFull);
    }
}
