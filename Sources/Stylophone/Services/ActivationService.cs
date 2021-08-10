using System;
using System.Threading.Tasks;

using Stylophone.Activation;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading;

namespace Stylophone.Services
{
    // For more information on understanding and extending activation flow see
    // https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/UWP/activation.md
    internal class ActivationService
    {
        private readonly App _app;
        private readonly Type _defaultNavItem;
        private Lazy<UIElement> _shell;

        private object _lastActivationArgs;

        public ActivationService(App app, Type defaultNavItem, Lazy<UIElement> shell = null)
        {
            _app = app;
            _shell = shell;
            _defaultNavItem = defaultNavItem;
        }

        public async Task ActivateAsync(object activationArgs)
        {
            if (IsInteractive(activationArgs))
            {
                // Initialize services that you need before app activation
                // take into account that the splash screen is shown while this code runs.
                await InitializeAsync();

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (Window.Current.Content == null)
                {
                    // Create a Shell or Frame to act as the navigation context
                    Window.Current.Content = _shell?.Value ?? new Frame();
                }
            }

            // Depending on activationArgs one of ActivationHandlers or DefaultActivationHandler
            // will navigate to the first page
            await HandleActivationAsync(activationArgs);
            _lastActivationArgs = activationArgs;

            if (IsInteractive(activationArgs))
            {
                // Ensure the current window is active
                Window.Current.Activate();

                // Tasks after activation
                await StartupAsync();
            }
        }

        private async Task InitializeAsync()
        {
            
        }

        private async Task HandleActivationAsync(object activationArgs)
        {
            if (IsInteractive(activationArgs))
            {
                var defaultHandler = new DefaultActivationHandler(_defaultNavItem, Ioc.Default.GetRequiredService<INavigationService>());
                if (defaultHandler.CanHandle(activationArgs))
                {
                    await defaultHandler.HandleAsync(activationArgs);
                }
            }
        }

        private async Task StartupAsync()
        {
            var theme = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ElementTheme));
            Enum.TryParse(theme, out Theme elementTheme);
            await Ioc.Default.GetRequiredService<IInteropService>().SetThemeAsync(elementTheme);

            await Ioc.Default.GetRequiredService<IDialogService>().ShowFirstRunDialogIfAppropriateAsync();

            _ = Task.Run(async () =>
            {
                Thread.Sleep(6000);
                await Ioc.Default.GetRequiredService<IDialogService>().ShowRateAppDialogIfAppropriateAsync();
            });

            var host = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ServerHost));
            var port = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<int>(nameof(SettingsViewModel.ServerPort), 6600);
            var pass = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ServerPassword));
            var localPlaybackEnabled = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.IsLocalPlaybackEnabled));

            var localPlaybackVm = Ioc.Default.GetRequiredService<LocalPlaybackViewModel>();
            localPlaybackVm.Initialize(host, localPlaybackEnabled);

            var mpdService = Ioc.Default.GetRequiredService<MPDConnectionService>();
            mpdService.SetServerInfo(host, port, pass);
            await mpdService.InitializeAsync(true);

            Ioc.Default.GetRequiredService<AlbumArtService>().Initialize();
            Ioc.Default.GetRequiredService<SystemMediaControlsService>().Initialize();
        }

        private bool IsInteractive(object args)
        {
            return args is IActivatedEventArgs;
        }
    }
}
