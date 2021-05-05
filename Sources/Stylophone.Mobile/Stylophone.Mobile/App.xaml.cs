using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using Stylophone.Mobile.Services;
using Stylophone.Mobile.ViewModels;
using Stylophone.Mobile.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Stylophone.Mobile
{
    public partial class App : Application
    {
        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        public App()
        {
            InitializeComponent();

            // Initialize IoC
            Services = ConfigureServices();
    
            try
            {
                Ioc.Default.ConfigureServices(Services);
            } 
            catch
            {
                // Ioc can be configured already on resume
            }
            

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {

            

            await Ioc.Default.GetRequiredService<IDialogService>().ShowFirstRunDialogIfAppropriateAsync();

            var host = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ServerHost));
            var port = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<int>(nameof(SettingsViewModel.ServerPort));

            var mpdService = Ioc.Default.GetRequiredService<MPDConnectionService>();
            mpdService.SetServerInfo(host, port);
            await mpdService.InitializeAsync(true);

            Ioc.Default.GetRequiredService<AlbumArtService>().Initialize();
            //Ioc.Default.GetRequiredService<SystemMediaControlsService>().Initialize();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
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
            services.AddSingleton<IInteropService, InteropService>();

            // Viewmodel Factories
            services.AddSingleton<AlbumViewModelFactory>();
            services.AddSingleton<TrackViewModelFactory>();
            services.AddSingleton<FilePathViewModelFactory>();

            // Viewmodels
            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<SettingsViewModel>();
            //services.AddSingleton<LibraryViewModel>();
            services.AddSingleton<AlbumDetailViewModel>();
            services.AddSingleton<FoldersViewModel>();
            services.AddSingleton<PlaylistViewModel>();
            services.AddSingleton<QueueViewModel>();
            services.AddSingleton<SearchResultsViewModel>();

            //services.AddTransient<PlaybackViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
