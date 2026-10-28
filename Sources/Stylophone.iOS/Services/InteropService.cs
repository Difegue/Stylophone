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
            // With multiple windows on iPad, prefer the window the user is currently interacting with
            var windowScenes = UIApplication.SharedApplication.ConnectedScenes.ToArray().OfType<UIWindowScene>().ToList();
            var scene = windowScenes.FirstOrDefault(s => s.ActivationState == UISceneActivationState.ForegroundActive)
                        ?? windowScenes.FirstOrDefault();

            return scene?.KeyWindow ?? scene?.Windows.FirstOrDefault() ?? (UIApplication.SharedApplication.Delegate as AppDelegate)?.Window;
        }

        /// <returns>The view controller to present dialogs from: the topmost one of the key window.</returns>
        public static UIViewController GetPresentingViewController()
        {
            var presenter = GetKeyWindow()?.RootViewController;
            while (presenter?.PresentedViewController != null && !presenter.PresentedViewController.IsBeingDismissed)
                presenter = presenter.PresentedViewController;

            return presenter;
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
            UIColor accent = UIColor.SystemBlue;
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                accent = GetKeyWindow()?.TintColor ?? UIColor.SystemBlue;
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
                PlaybackIcon.VolumeMute => "speaker.slash.fill",
                PlaybackIcon.Volume25 => "speaker.fill",
                PlaybackIcon.Volume50 => "speaker.wave.1.fill",
                PlaybackIcon.Volume75 => "speaker.wave.2.fill",
                PlaybackIcon.VolumeFull => "speaker.wave.3.fill",
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
