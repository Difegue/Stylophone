using Foundation;
using UIKit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using System.Threading.Tasks;
using Stylophone.iOS.Services;
using Stylophone.iOS.ViewModels;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace Stylophone.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIResponder, IUIApplicationDelegate
    {
        public IServiceProvider Services { get; }

        [Export("window")]
        public UIWindow Window { get; set; }

        public UISplitViewController RootViewController { get; set; }

        public AppDelegate()
        {
            Services = ConfigureServices();
            Ioc.Default.ConfigureServices(Services);
        }

        public void ShowDetailView()
        {
            // Make sure the detail view is showing
            RootViewController?.ShowColumn(UISplitViewControllerColumn.Secondary);
        }

        [Export("application:didFinishLaunchingWithOptions:")]
        public bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Override point for customization after application launch
            Task.Run(async () => await InitializeApplicationAsync());

            return true;
        }

        private async Task InitializeApplicationAsync()
        {
            var host = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ServerHost));
            var port = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<int>(nameof(SettingsViewModel.ServerPort));
            var pass = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ServerPassword));

            var mpdService = Ioc.Default.GetRequiredService<MPDConnectionService>();
            mpdService.SetServerInfo(host, port, pass);
            await mpdService.InitializeAsync(true);

            Ioc.Default.GetRequiredService<AlbumArtService>().Initialize();

            await Ioc.Default.GetRequiredService<IDispatcherService>().ExecuteOnUIThreadAsync(async () =>
            {
                var theme = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ElementTheme));
                Enum.TryParse(theme, out Theme elementTheme);
                await Ioc.Default.GetRequiredService<IInteropService>().SetThemeAsync(elementTheme);

                await Ioc.Default.GetRequiredService<IDialogService>().ShowFirstRunDialogIfAppropriateAsync();
            });

            // Analytics
            var enableAnalytics = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.EnableAnalytics));
            if (enableAnalytics)
            {
                // Initialize AppCenter
                AppCenter.Start("90b62f5a-2448-4ef1-81ca-3fb807a5b126",
                   typeof(Analytics), typeof(Crashes));
            }
        }

        // UISceneSession Lifecycle

        [Export("application:configurationForConnectingSceneSession:options:")]
        public UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
        {
            // Called when a new scene session is being created.
            // Use this method to select a configuration to create the new scene with.
            return UISceneConfiguration.Create("Default Configuration", connectingSceneSession.Role);
        }

        [Export("application:didDiscardSceneSessions:")]
        public void DidDiscardSceneSessions(UIApplication application, NSSet<UISceneSession> sceneSessions)
        {
            // Called when the user discards a scene session.
            // If any sessions were discarded while the application was not running, this will be called shortly after `FinishedLaunching`.
            // Use this method to release any resources that were specific to the discarded scenes, as they will not return.
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
            //services.AddSingleton<SystemMediaControlsService>();
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

