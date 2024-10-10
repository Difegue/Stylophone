using System;
using System.Threading.Tasks;

using Stylophone.Common.Interfaces;
using SkiaSharp;
using Stylophone.Common.ViewModels;
using UIKit;
using SkiaSharp.Views.iOS;
using Foundation;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Stylophone.iOS.Services
{
    public class InteropService: IInteropService
    {
        public InteropService()
        {
        }

        public static UIWindow GetKeyWindow()
        {
            return UIApplication.SharedApplication.ConnectedScenes.ToArray()
                    .Select(s => (UIWindowScene)s)
                    .First().Windows.Where(w => w.IsKeyWindow).First();
        }

        public Task SetThemeAsync(Theme theme)
        {
            var keyWindow = GetKeyWindow();

            switch (theme)
            {
                case Theme.Dark:
                    keyWindow.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Dark;
                    return Task.CompletedTask;
                case Theme.Light:
                    keyWindow.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Light;
                    return Task.CompletedTask;
                default:
                    keyWindow.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Unspecified;
                    return Task.CompletedTask;
            }
        }

        public SKColor GetAccentColor()
        {
            var keyWindow = GetKeyWindow();
            UIColor accent = UIColor.SystemBlue;
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                accent = keyWindow.TintColor ?? UIColor.SystemBlue;
            });
            return accent.ToSKColor();
        }

        public async Task<SKImage> GetPlaceholderImageAsync()
        {
            var tcs = new TaskCompletionSource<UIImage>();
            UIApplication.SharedApplication.BeginInvokeOnMainThread(() =>
            {
                var imageFile = UIImage.FromBundle("AlbumPlaceholder");
                tcs.SetResult(imageFile);
            });

            var imageFile = await tcs.Task;
            var skImage = imageFile.ToSKImage();
            return skImage;
        }

        public async Task UpdateOperatingSystemIntegrationsAsync(TrackViewModel currentTrack)
        {
            // This needs to be instantiated lazily since NPService depends on LocalPlayback, which depends on Interop already..
            var nowPlayingService = Ioc.Default.GetRequiredService<NowPlayingService>();
            await nowPlayingService.UpdateMetadataAsync(currentTrack);
        }

        public Version GetAppVersion()
        {
            var bundleVersion = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString");
            return new Version(bundleVersion.ToString());
        }

        public string GetIcon(PlaybackIcon icon)
        {
            return icon switch
            {
                PlaybackIcon.Play => "play.fill",
                PlaybackIcon.Pause => "pause.fill",
                PlaybackIcon.RepeatOff => "repeat.circle",
                PlaybackIcon.Repeat => "repeat.circle.fill",
                PlaybackIcon.RepeatSingle => "repeat.1.circle.fill",
                PlaybackIcon.VolumeMute => "speaker.slash.circle",
                PlaybackIcon.Volume25 => "speaker.wave.2.circle",
                PlaybackIcon.Volume50 => "speaker.wave.2.circle",
                PlaybackIcon.Volume75 => "speaker.wave.2.circle",
                PlaybackIcon.VolumeFull => "speaker.wave.2.circle",
                _ => "opticaldisc",
            };
        }

        public Task OpenStoreReviewUrlAsync()
        {
            try
            {
                UIApplication.SharedApplication.OpenUrl(new NSUrl("https://apps.apple.com/app/id1644672889?action=write-review"),new UIApplicationOpenUrlOptions() { OpenInPlace = true }, null);
            } catch { }
            return Task.CompletedTask;
        }
    }
}
