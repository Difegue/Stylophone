using System;
using System.Threading.Tasks;
using Stylophone.Services;
using Stylophone.Common.Interfaces;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using System.Threading;
using Microsoft.Toolkit.Uwp.Helpers;
using Sentry;
using System.Reflection;

namespace Stylophone.Activation
{
    internal class DefaultActivationHandler : ActivationHandler<IActivatedEventArgs>
    {
        private readonly Type _navElement;

        private INavigationService _navigationService;

        public DefaultActivationHandler(Type navElement, INavigationService navigationService)
        {
            _navElement = navElement;
            _navigationService = navigationService;
        }

        protected override async Task HandleInternalAsync(IActivatedEventArgs args)
        {
            // When the navigation stack isn't restored, navigate to the first page and configure
            // the new page by passing required information in the navigation parameter
            object arguments = null;
            if (args is LaunchActivatedEventArgs launchArgs)
            {
                arguments = launchArgs.Arguments;
            }

            _navigationService.Navigate(_navElement, arguments);

            // Ensure the current window is active
            Window.Current.Activate();

            // Tasks after activation
            await StartupAsync();
        }

        private async Task StartupAsync()
        {
            var theme = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ElementTheme));
            Enum.TryParse(theme, out Theme elementTheme);
            await Ioc.Default.GetRequiredService<IInteropService>().SetThemeAsync(elementTheme);

            await Ioc.Default.GetRequiredService<IDialogService>().ShowFirstRunDialogIfAppropriateAsync();

            _ = Task.Run(async () =>
            {
                Thread.Sleep(60000);
                await Ioc.Default.GetRequiredService<IDialogService>().ShowRateAppDialogIfAppropriateAsync();
            });

            var host = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ServerHost));
            host = host?.Replace("\"", ""); // TODO: This is a quickfix for 1.x updates
            var port = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<int>(nameof(SettingsViewModel.ServerPort), 6600);
            var pass = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ServerPassword));
            var localPlaybackEnabled = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.IsLocalPlaybackEnabled));
            var localPlaybackPort = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<int>(nameof(SettingsViewModel.LocalPlaybackPort), 8000);

            var localPlaybackVm = Ioc.Default.GetRequiredService<LocalPlaybackViewModel>();
            localPlaybackVm.Initialize(host, localPlaybackPort, localPlaybackEnabled);

            var mpdService = Ioc.Default.GetRequiredService<MPDConnectionService>();
            mpdService.SetServerInfo(host, port, pass);
            await mpdService.InitializeAsync(true);

            Ioc.Default.GetRequiredService<AlbumArtService>().Initialize();
            Ioc.Default.GetRequiredService<SystemMediaControlsService>().Initialize();

#if DEBUG
#else
            var enableAnalytics = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.EnableAnalytics), true);
            if (enableAnalytics)
            {
                var assembly = Assembly.GetExecutingAssembly().GetName();
                var isUnpackaged = false;

                SentrySdk.Init(options =>
                {
                    options.Dsn = "https://e0fe6745e1dd14e850ade2518e7080b3@o4508492455149568.ingest.de.sentry.io/4508549239537744";
                });

                SentrySdk.ConfigureScope(scope =>
                {
                    scope.Release = assembly.Version.ToString();

                    try
                    {
                        scope.Release = SystemInformation.Instance.ApplicationVersion.ToFormattedString();
                        scope.Contexts.Device.Brand = SystemInformation.Instance.DeviceManufacturer;
                        scope.Contexts.Device.Name = SystemInformation.Instance.DeviceModel;
                        scope.Contexts.Device.Family = SystemInformation.Instance.DeviceFamily;
                        scope.Contexts.Device.Architecture = SystemInformation.Instance.OperatingSystemArchitecture.ToString();

                        scope.SetTag("total_launch_count", SystemInformation.Instance.TotalLaunchCount.ToString());
                        scope.SetTag("unpackaged", "false");
                    }
                    catch
                    {
                        // SystemInformation will fail in unpackaged scenarios
                        scope.SetTag("unpackaged", "true");
                        isUnpackaged = true;
                    }
                });

                SentrySdk.CaptureMessage($"{assembly.Name} - {assembly.Version} " + (isUnpackaged ? "(Unpackaged)" : ""));
            }
#endif 
        }

        protected override bool CanHandleInternal(IActivatedEventArgs args)
        {
            // None of the ActivationHandlers has handled the app activation
            return ((NavigationService)_navigationService).Frame?.Content == null && _navElement != null;
        }
    }
}
