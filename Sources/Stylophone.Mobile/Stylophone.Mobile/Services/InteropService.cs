using SkiaSharp;
using SkiaSharp.Views.Forms;
using Stylophone.Common.Interfaces;
using Stylophone.Common.ViewModels;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Stylophone.Mobile.Services
{
    public class InteropService : IInteropService
    {
        public SKColor GetAccentColor() => Color.Accent.ToSKColor();

        public Version GetAppVersion() => AppInfo.Version;

        public Task<SKImage> GetPlaceholderImageAsync()
        {
            // todo
            var image = SKImage.Create(new SKImageInfo(100, 100));
            return new Task<SKImage>(() => image);
        }

        public Task SetThemeAsync(Theme param)
        {
            Application.Current.UserAppTheme = (OSAppTheme)(int)param;
            return Task.CompletedTask;
        }

        public Task UpdateOperatingSystemIntegrationsAsync(TrackViewModel currentTrack)
        {
            return Task.CompletedTask;
        }
    }
}
