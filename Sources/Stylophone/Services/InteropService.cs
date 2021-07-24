using System;
using System.Threading.Tasks;

using Stylophone.Common.Interfaces;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using SkiaSharp;
using Stylophone.Common.ViewModels;
using System.IO;
using Windows.ApplicationModel;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace Stylophone.Services
{
    public class InteropService : IInteropService
    {
        private ApplicationTheme _appTheme;
        private SystemMediaControlsService _smtcService;
        private MediaPlayer _mediaPlayer;

        public InteropService(SystemMediaControlsService smtcService)
        {
            _smtcService = smtcService;

            UISettings uiSettings = new UISettings();
            uiSettings.ColorValuesChanged += HandleSystemThemeChange;

            // Fallback in case the above fails, we'll check when we get activated next.
            Window.Current.CoreWindow.Activated += CoreWindow_Activated;
        }

        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            if (Window.Current.Content is FrameworkElement frameworkElement && _appTheme != Application.Current.RequestedTheme)
            {
                UpdateTitleBar(frameworkElement.RequestedTheme);
            }
        }

        private void HandleSystemThemeChange(UISettings sender, object args)
        {
            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                UpdateTitleBar(frameworkElement.RequestedTheme);
            }  
        }

        public async Task SetThemeAsync(Theme theme)
        {
            await SetRequestedThemeAsync(GetTheme(theme));
        }

        public SKColor GetAccentColor()
        {
            var accent = (Color)Application.Current.Resources["SystemAccentColor"];

            return new SKColor(accent.R, accent.G, accent.B, accent.A);
        }

        public async Task<SKImage> GetPlaceholderImageAsync()
        {
            var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/AlbumPlaceholder.png"));

            var stream = await imageFile.OpenStreamForReadAsync();
            var bitmap = SKBitmap.Decode(stream);
            stream.Close();

            return SKImage.FromBitmap(bitmap);
        }

        public async Task UpdateOperatingSystemIntegrationsAsync(TrackViewModel currentTrack)
        {
            await _smtcService.UpdateMetadataAsync(currentTrack);
        }

        public Version GetAppVersion()
        {
            var package = Package.Current;
            var packageId = package.Id;
            return new Version(packageId.Version.Major, packageId.Version.Minor, packageId.Version.Revision, packageId.Version.Build);
        }


        private async Task SetRequestedThemeAsync(ElementTheme theme)
        {
            foreach (var view in CoreApplication.Views)
            {
                await view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (Window.Current.Content is FrameworkElement frameworkElement)
                    {
                        frameworkElement.RequestedTheme = theme;
                        UpdateTitleBar(theme);
                    }
                });
            }
        }

        private void UpdateTitleBar(ElementTheme theme)
        {
            // https://stackoverflow.com/questions/48201278/uwp-changing-titlebar-buttonforegroundcolor-with-themeresource
            Color color;
            _appTheme = Application.Current.RequestedTheme;

            switch (theme)
            {
                case ElementTheme.Default:
                    color = ((Color)Application.Current.Resources["SystemBaseHighColor"]);
                    break;
                case ElementTheme.Light:
                    if (_appTheme == ApplicationTheme.Light) { color = ((Color)Application.Current.Resources["SystemBaseHighColor"]); }
                    else { color = ((Color)Application.Current.Resources["SystemAltHighColor"]); }
                    break;
                case ElementTheme.Dark:
                    if (_appTheme == ApplicationTheme.Light) { color = ((Color)Application.Current.Resources["SystemAltHighColor"]); }
                    else { color = ((Color)Application.Current.Resources["SystemBaseHighColor"]); }
                    break;
                default:
                    break;
            }

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonForegroundColor = color;
        }

        private ElementTheme GetTheme(Theme theme)
        {
            return theme switch
            {
                Theme.Default => ElementTheme.Default,
                Theme.Light => ElementTheme.Light,
                Theme.Dark => ElementTheme.Dark,
                _ => throw new NotImplementedException(),
            };
        }

        public string GetIcon(PlaybackIcon icon)
        {
            return icon switch
            {
                PlaybackIcon.Play => "\uF5B0",
                PlaybackIcon.Pause => "\uF8AE",
                PlaybackIcon.Repeat => "\uE8EE",
                PlaybackIcon.RepeatSingle => "\uE8ED",
                PlaybackIcon.RepeatOff => "\uE8EE",
                PlaybackIcon.VolumeMute => "\uE74F",
                PlaybackIcon.Volume25 => "\uE993",
                PlaybackIcon.Volume50 => "\uE994",
                PlaybackIcon.Volume75 => "\uE767",
                PlaybackIcon.VolumeFull => "\uE767",
                _ => "?",
            };
        }
        public void PlayStream(Uri streamUri)
        {
            if (_mediaPlayer == null)
            {
                _mediaPlayer = new MediaPlayer();
                _mediaPlayer.RealTimePlayback = true;
                _mediaPlayer.CommandManager.IsEnabled = false; // Disable SMTC integration, as we use it to control the MPD server instead
            }

            _mediaPlayer.Source = MediaSource.CreateFromUri(streamUri);
            _mediaPlayer.Play();
        }

        public void StopStream()
        {
            _mediaPlayer?.Pause();
        }

        public void SetStreamVolume(double volume)
        {
            if (_mediaPlayer != null)
                _mediaPlayer.Volume = volume / 100;
        }
    }
}
