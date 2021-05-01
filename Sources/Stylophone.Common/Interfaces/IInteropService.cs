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

    public interface IInteropService
    {
        SKColor GetAccentColor();
        Task<SKImage> GetPlaceholderImageAsync();
        Task UpdateOperatingSystemIntegrationsAsync(TrackViewModel currentTrack);
        Version GetAppVersion();
        Task SetThemeAsync(Theme param);
    }
}
