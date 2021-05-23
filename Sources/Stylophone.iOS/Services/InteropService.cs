using System;
using System.Threading.Tasks;

using Stylophone.Common.Interfaces;
using SkiaSharp;
using Stylophone.Common.ViewModels;
using UIKit;
using SkiaSharp.Views.iOS;
using Foundation;

namespace Stylophone.iOS.Services
{
    public class InteropService: IInteropService
    {

        //private SystemMediaControlsService _smtcService;

        public InteropService( )
        {
        }

        public async Task SetThemeAsync(Theme theme)
        {
            switch (theme)
            {
                case Theme.Dark:
                    UIApplication.SharedApplication.KeyWindow.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Dark;
                    return;
                case Theme.Light:
                    UIApplication.SharedApplication.KeyWindow.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Light;
                    return;
                default:
                    UIApplication.SharedApplication.KeyWindow.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Unspecified;
                    return;
            }
        }

        public SKColor GetAccentColor()
        {
            var accent = UIColor.SystemBlueColor;
            return accent.ToSKColor();
        }

        public async Task<SKImage> GetPlaceholderImageAsync()
        {
            // TODO
            var tcs = new TaskCompletionSource<UIImage>();
            UIApplication.SharedApplication.BeginInvokeOnMainThread(() =>
            {
                var imageFile = UIImage.GetSystemImage("tv.music.note.fill");
                tcs.SetResult(imageFile);
            });

            var imageFile = await tcs.Task;
            var skImage = imageFile.ToSKImage();
            return skImage;
        }

        public async Task UpdateOperatingSystemIntegrationsAsync(TrackViewModel currentTrack)
        {
            //await _smtcService.UpdateMetadataAsync(currentTrack);
            //await LiveTileHelper.UpdatePlayingSongAsync(currentTrack);
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
                PlaybackIcon.Repeat => "repeat",
                PlaybackIcon.RepeatSingle => "repeat.1",
                PlaybackIcon.VolumeMute => "speaker.slash.circle",
                PlaybackIcon.Volume25 => "speaker.wave.2.circle",
                PlaybackIcon.Volume50 => "speaker.wave.2.circle",
                PlaybackIcon.Volume75 => "speaker.wave.2.circle",
                PlaybackIcon.VolumeFull => "speaker.wave.2.circle",
                _ => "opticaldisc",
            };
        }
    }
}
