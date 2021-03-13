using System;
using System.Threading.Tasks;

using FluentMPC.Helpers;

using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace FluentMPC.Services
{
    public static class ThemeSelectorService
    {
        private const string SettingsKey = "AppBackgroundRequestedTheme";

        public static ElementTheme Theme { get; set; } = ElementTheme.Default;

        public static async Task InitializeAsync()
        {
            Theme = await LoadThemeFromSettingsAsync();
        }

        public static async Task SetThemeAsync(ElementTheme theme)
        {
            Theme = theme;

            await SetRequestedThemeAsync();
            await SaveThemeInSettingsAsync(Theme);
        }

        public static async Task SetRequestedThemeAsync()
        {
            foreach (var view in CoreApplication.Views)
            {
                await view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (Window.Current.Content is FrameworkElement frameworkElement)
                    {
                        frameworkElement.RequestedTheme = Theme;

                        // https://stackoverflow.com/questions/48201278/uwp-changing-titlebar-buttonforegroundcolor-with-themeresource
                        Color color;
                        var appTheme = Application.Current.RequestedTheme;

                        switch (Theme)
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

        private static async Task<ElementTheme> LoadThemeFromSettingsAsync()
        {
            ElementTheme cacheTheme = ElementTheme.Default;
            string themeName = await ApplicationData.Current.LocalSettings.ReadAsync<string>(SettingsKey);

            if (!string.IsNullOrEmpty(themeName))
            {
                Enum.TryParse(themeName, out cacheTheme);
            }

            return cacheTheme;
        }

        private static async Task SaveThemeInSettingsAsync(ElementTheme theme)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync(SettingsKey, theme.ToString());
        }
    }
}
