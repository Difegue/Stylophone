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
        Play, 
        Pause,
        Repeat,
        RepeatSingle,
        VolumeMute,
        Volume25,
        Volume50,
        Volume75,
        VolumeFull
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
