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
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Threading;
using AVFoundation;
using System.Collections.Generic;

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

        public event EventHandler<EventArgs> ApplicationWillResign;
        public event EventHandler<EventArgs> ApplicationWillBecomeActive;

        public UISplitViewController RootViewController { get; set; }

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
            
            // Override point for customization after application launch
            Task.Run(async () => await InitializeApplicationAsync());

            RootViewController = Window.RootViewController as UISplitViewController;
            Window.TintColor = AppColor;

            return true;
        }

        [Export("applicationWillResignActive:")]
        public void OnResignActivation(UIApplication application)
        {
            ApplicationWillResign?.Invoke(this, EventArgs.Empty);
        }

        [Export("applicationDidBecomeActive:")]
        public void OnActivated(UIApplication application)
        {
            ApplicationWillBecomeActive?.Invoke(this, EventArgs.Empty);
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

            var launchCount = storageService.GetValue<int>("LaunchCount");
            storageService.SetValue("LaunchCount", launchCount + 1);

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
                // Initialize AppCenter
                AppCenter.Start("90b62f5a-2448-4ef1-81ca-3fb807a5b126",
                   typeof(Analytics), typeof(Crashes));

                AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                    var dict = new Dictionary<string, string>();
                    dict.Add("exception", args.ExceptionObject.ToString());
                    Analytics.TrackEvent("UnhandledCrash", dict);
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

