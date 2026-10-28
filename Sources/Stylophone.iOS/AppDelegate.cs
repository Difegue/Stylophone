using Foundation;
using UIKit;

using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using System.Threading.Tasks;
using Stylophone.iOS.Services;
using Stylophone.iOS.ViewModels;
using Sentry;
using System.Threading;
using AVFoundation;
using System.Collections.Generic;
using System.Reflection;

namespace Stylophone.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIResponder, IUIApplicationDelegate
    {
        public IServiceProvider Services { get; }

        // Main window, owned by the main scene (see SceneDelegate)
        public UIWindow Window { get; set; }

        public event EventHandler<EventArgs> ApplicationWillResign;
        public event EventHandler<EventArgs> ApplicationWillBecomeActive;

        public UISplitViewController RootViewController { get; set; }

        private NSObject _willResignActiveObserver;
        private NSObject _didBecomeActiveObserver;

        public UIColor AppColor => UIColor.FromDynamicProvider((traitCollection) =>
        {
            var darkColor = UIColor.FromRGB(204, 172, 128);
            var lightColor = UIColor.FromRGB(135, 114, 85);

            return traitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark ? darkColor : lightColor;
        });

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
            // Enable Now Playing integration
            application.BeginReceivingRemoteControlEvents();
            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Playback);
            //AVAudioSession.SharedInstance().SetPrefersNoInterruptionsFromSystemAlerts(true, out _);

            // With the UIScene lifecycle, activation events go to the scene delegates instead of the app delegate.
            // The app-wide notifications are still posted though, so use those to keep our events app-wide.
            _willResignActiveObserver = UIApplication.Notifications.ObserveWillResignActive((s, e) => ApplicationWillResign?.Invoke(this, EventArgs.Empty));
            _didBecomeActiveObserver = UIApplication.Notifications.ObserveDidBecomeActive((s, e) => ApplicationWillBecomeActive?.Invoke(this, EventArgs.Empty));

            // Override point for customization after application launch
            Task.Run(async () => await InitializeApplicationAsync());

            return true;
        }

        {

        }

        public override void BuildMenu(IUIMenuBuilder builder)
        {
            base.BuildMenu(builder);

            builder.RemoveMenu(UIMenuIdentifier.Edit.GetConstant());
            builder.RemoveMenu(UIMenuIdentifier.Font.GetConstant());
            builder.RemoveMenu(UIMenuIdentifier.Format.GetConstant());
            builder.RemoveMenu(UIMenuIdentifier.Services.GetConstant());
            builder.RemoveMenu(UIMenuIdentifier.Hide.GetConstant());

            builder.RemoveMenu(UIMenuIdentifier.File.GetConstant());
            builder.RemoveMenu(UIMenuIdentifier.Document.GetConstant());

            builder.System.SetNeedsRebuild();
        }

        private async Task InitializeApplicationAsync()
        {
            var storageService = Ioc.Default.GetRequiredService<IApplicationStorageService>();

            var host = storageService.GetValue<string>(nameof(SettingsViewModel.ServerHost));
            var port = storageService.GetValue<int>(nameof(SettingsViewModel.ServerPort), 6600);
            var pass = storageService.GetValue<string>(nameof(SettingsViewModel.ServerPassword));

            var localPlaybackEnabled = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.IsLocalPlaybackEnabled));
            var localPlaybackPort = storageService.GetValue<int>(nameof(SettingsViewModel.LocalPlaybackPort), 8000);
            var localPlaybackVm = Ioc.Default.GetRequiredService<LocalPlaybackViewModel>();
            localPlaybackVm.Initialize(host, localPlaybackPort, localPlaybackEnabled);

            var mpdService = Ioc.Default.GetRequiredService<MPDConnectionService>();
            mpdService.SetServerInfo(host, port, pass);
            await mpdService.InitializeAsync(true);

            var launchCount = storageService.GetValue<int>("LaunchCount") + 1;
            storageService.SetValue("LaunchCount", launchCount);

            Ioc.Default.GetRequiredService<AlbumArtService>().Initialize();
            Ioc.Default.GetRequiredService<NowPlayingService>().Initialize();

            await Ioc.Default.GetRequiredService<IDispatcherService>().ExecuteOnUIThreadAsync(async () =>
            {
                var theme = storageService.GetValue<string>(nameof(SettingsViewModel.ElementTheme));
                Enum.TryParse(theme, out Theme elementTheme);
                await Ioc.Default.GetRequiredService<IInteropService>().SetThemeAsync(elementTheme);

                await Ioc.Default.GetRequiredService<IDialogService>().ShowFirstRunDialogIfAppropriateAsync();
            });

            _ = Task.Run(async () =>
            {
                Thread.Sleep(60000);
                await Ioc.Default.GetRequiredService<IDialogService>().ShowRateAppDialogIfAppropriateAsync();
            });

#if DEBUG
#else
            // Analytics
            var enableAnalytics = storageService.GetValue<bool>(nameof(SettingsViewModel.EnableAnalytics), true);
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
                    scope.SetTag("total_launch_count", launchCount);
                });

                SentrySdk.CaptureMessage($"{assembly.Name} - {assembly.Version} ");

                AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                    SentrySdk.CaptureException(args.ExceptionObject as Exception);
                };
            }
#endif

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
            services.AddSingleton<NowPlayingService>();
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

