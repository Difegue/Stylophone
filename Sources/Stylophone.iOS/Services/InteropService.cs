using System;
using System.Threading.Tasks;

using Stylophone.Common.Interfaces;
using SkiaSharp;
using Stylophone.Common.ViewModels;
using UIKit;
using SkiaSharp.Views.iOS;
using Foundation;
using AVFoundation;

namespace Stylophone.iOS.Services
{
    public class InteropService: IInteropService
    {
        private AVPlayer _mediaPlayer;

        public InteropService( )
        {
        }

        public Task SetThemeAsync(Theme theme)
        {
            switch (theme)
            {
                case Theme.Dark:
                    UIApplication.SharedApplication.KeyWindow.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Dark;
                    return Task.CompletedTask;
                case Theme.Light:
                    UIApplication.SharedApplication.KeyWindow.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Light;
                    return Task.CompletedTask;
                default:
                    UIApplication.SharedApplication.KeyWindow.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Unspecified;
                    return Task.CompletedTask;
            }
        }

        public SKColor GetAccentColor()
        {
            var accent = UIApplication.SharedApplication.KeyWindow.TintColor;
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

        public Task UpdateOperatingSystemIntegrationsAsync(TrackViewModel currentTrack)
        {
            //await _smtcService.UpdateMetadataAsync(currentTrack);
            //await LiveTileHelper.UpdatePlayingSongAsync(currentTrack);
            return Task.CompletedTask;
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

        public void PlayStream(Uri streamUri)
        {
            if (_mediaPlayer == null)
                _mediaPlayer = new AVPlayer();

            var playerItem = new AVPlayerItem(new NSUrl(streamUri.AbsoluteUri));
            _mediaPlayer.ReplaceCurrentItemWithPlayerItem(playerItem);

            _mediaPlayer.Play();
        }

        public void StopStream()
        {
            _mediaPlayer.Pause();
        }

        public void SetStreamVolume(double volume)
        {
            if (_mediaPlayer != null)
                _mediaPlayer.Volume = (float)volume;
        }

        public Task OpenStoreReviewUrlAsync()
        {
            try
            {
                UIApplication.SharedApplication.OpenUrl(new NSUrl("https://apps.apple.com/app/idXXXXXXXXXX?action=write-review"));
            } catch { }
            return Task.CompletedTask;
        }
    }
}
