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

namespace Stylophone.Services
{
    public class InteropService: IInteropService
    {

        private SystemMediaControlsService _smtcService;

        public InteropService(SystemMediaControlsService smtcService)
        {
            _smtcService = smtcService;
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
            await LiveTileHelper.UpdatePlayingSongAsync(currentTrack);
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

                        // https://stackoverflow.com/questions/48201278/uwp-changing-titlebar-buttonforegroundcolor-with-themeresource
                        Color color;
                        var appTheme = Application.Current.RequestedTheme;

                        switch (theme)
                        {
                            case ElementTheme.Default:
                                color = ((Color)Application.Current.Resources["SystemBaseHighColor"]);
                                break;
                            case ElementTheme.Light:
                                if (appTheme == ApplicationTheme.Light) { color = ((Color)Application.Current.Resources["SystemBaseHighColor"]); }
                                else { color = ((Color)Application.Current.Resources["SystemAltHighColor"]); }
                                break;
                            case ElementTheme.Dark:
                                if (appTheme == ApplicationTheme.Light) { color = ((Color)Application.Current.Resources["SystemAltHighColor"]); }
                                else { color = ((Color)Application.Current.Resources["SystemBaseHighColor"]); }
                                break;
                            default:
                                break;
                        }

                        ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
                        titleBar.ButtonForegroundColor = color;
                    }
                });
            }
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
    }
}
