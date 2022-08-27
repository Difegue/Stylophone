using System;
using Stylophone.Services;
using Stylophone.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.DependencyInjection;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Foundation;
using System.Collections.Generic;

namespace Stylophone
{
    public sealed partial class App : Application
    {
        private Lazy<ActivationService> _activationService;

        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        public App()
        {
            // Initialize IoC
            Services = ConfigureServices();
            Ioc.Default.ConfigureServices(Services);

            InitializeComponent();
            UnhandledException += OnAppUnhandledException;

            // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Windows.ApplicationModel.Core.CoreApplication.EnablePrelaunch(true);

            // https://docs.microsoft.com/en-us/windows/uwp/design/devices/designing-for-tv#custom-visual-state-trigger-for-xbox
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500,500));

            // Compact sizing
            var isCompactEnabled = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.IsCompactSizing));
            if (isCompactEnabled)
            {
                Resources.MergedDictionaries.Add(
                   new ResourceDictionary { Source = new Uri(@"ms-appx:///Microsoft.UI.Xaml/DensityStyles/Compact.xaml", UriKind.Absolute) });
            }

            // Analytics
            SystemInformation.Instance.TrackAppUse(args);
#if DEBUG
#else
            var enableAnalytics = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.EnableAnalytics), true);
            if (enableAnalytics)
            {
                // Initialize AppCenter
                AppCenter.Start("f2193544-6a38-42f6-92bd-69964bc3a0e8",
                    typeof(Analytics), typeof(Crashes));
            }
#endif

            var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonForegroundColor = (Color)Resources["SystemBaseHighColor"];
            viewTitleBar.ButtonInactiveForegroundColor = (Color)Resources["SystemBaseHighColor"];

            if (!args.PrelaunchActivated)
            {
                await ActivationService.ActivateAsync(args);
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        private void OnAppUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
#if DEBUG
#else
            var enableAnalytics = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.EnableAnalytics), true);
            if (enableAnalytics)
            {
                var dict = new Dictionary<string, string>();
                dict.Add("exception", e.Exception.ToString());
                Analytics.TrackEvent("UnhandledCrash", dict);
            }
#endif
            var notificationService = Ioc.Default.GetRequiredService<INotificationService>();
            notificationService.ShowErrorNotification(e.Exception);

            // Try to handle the exception in case it's not catastrophic
            e.Handled = true;
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(QueueViewModel), new Lazy<UIElement>(CreateShell));
        }

        private UIElement CreateShell()
        {
            return new Views.ShellPage();
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Services
            services.AddSingleton<IDispatcherService, DispatcherService>();
            services.AddSingleton<IApplicationStorageService, ApplicationStorageService>();
            services.AddSingleton<MPDConnectionService>();
            services.AddSingleton<AlbumArtService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<SystemMediaControlsService>();
            services.AddSingleton<IInteropService, InteropService>();

            // Viewmodel Factories
            services.AddSingleton<AlbumViewModelFactory>();
            services.AddSingleton<TrackViewModelFactory>();
            services.AddSingleton<FilePathViewModelFactory>();

            // Viewmodels
            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<LibraryViewModel>();
            services.AddSingleton<AlbumDetailViewModel>();
            services.AddSingleton<FoldersViewModel>();
            services.AddSingleton<PlaylistViewModel>();
            services.AddSingleton<QueueViewModel>();
            services.AddSingleton<SearchResultsViewModel>();
            services.AddSingleton<LocalPlaybackViewModel>();

            services.AddTransient<PlaybackViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
