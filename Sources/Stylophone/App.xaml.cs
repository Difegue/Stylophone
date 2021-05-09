using System;
using Stylophone.Services;
using Stylophone.ViewModels;
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
using MvvmCross.Platforms.Uap.Views;
using MvvmCross.Platforms.Uap.Core;
using MvvmCross;
using MvvmCross.Platforms.Uap.Presenters;
using System.Collections.Generic;
using System.Reflection;

namespace Stylophone
{

    public abstract class StylophoneApp : MvxApplication<StylophoneSetup, Common.App>
    {
    }

    public class StylophoneSetup: MvxWindowsSetup<Common.App>
    {
        protected override void InitializeFirstChance()
        {
            base.InitializeFirstChance();

            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Windows.ApplicationModel.Core.CoreApplication.EnablePrelaunch(true);

            var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonForegroundColor = (Color)Application.Current.Resources["SystemBaseHighColor"];
            viewTitleBar.ButtonInactiveForegroundColor = (Color)Application.Current.Resources["SystemBaseHighColor"];

            // https://docs.microsoft.com/en-us/windows/uwp/design/devices/designing-for-tv#custom-visual-state-trigger-for-xbox
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            // Platform-specific Services
            Mvx.IoCProvider.RegisterSingleton<IDispatcherService>(new DispatcherService());
            Mvx.IoCProvider.RegisterSingleton<IApplicationStorageService>(new ApplicationStorageService());
            Mvx.IoCProvider.RegisterSingleton<IDialogService>(() => Mvx.IoCProvider.IoCConstruct<DialogService>());
            Mvx.IoCProvider.RegisterSingleton<INotificationService>(() => Mvx.IoCProvider.IoCConstruct<NotificationService>());
            Mvx.IoCProvider.RegisterSingleton(() => Mvx.IoCProvider.IoCConstruct<SystemMediaControlsService>());
            Mvx.IoCProvider.RegisterSingleton<IInteropService>(() => Mvx.IoCProvider.IoCConstruct<InteropService>());

            // Platform-specific Viewmodels
            Mvx.IoCProvider.RegisterSingleton(() => Mvx.IoCProvider.IoCConstruct<LibraryViewModel>());

            // Viewmodel Factories
            Mvx.IoCProvider.RegisterSingleton(() => Mvx.IoCProvider.IoCConstruct<AlbumViewModelFactory>());
            Mvx.IoCProvider.RegisterSingleton(() => Mvx.IoCProvider.IoCConstruct<TrackViewModelFactory>());
            Mvx.IoCProvider.RegisterSingleton(() => Mvx.IoCProvider.IoCConstruct<FilePathViewModelFactory>());
        }

        protected override IMvxWindowsViewPresenter CreateViewPresenter(IMvxWindowsFrame rootFrame)
        {
            var presenter = new StylophoneViewPresenter(rootFrame);
            //presenter.AddPresentationHintHandler<ShellFrameHint>(hint => SetShellFrame(hint));
            return presenter;
        }

        public override IEnumerable<Assembly> GetViewModelAssemblies()
        {
            var list = new List<Assembly>();
            list.AddRange(base.GetViewModelAssemblies());
            list.Add(typeof(ShellViewModel).GetTypeInfo().Assembly);

            return list;
        }

        protected override async void InitializeLastChance()
        {
            base.InitializeLastChance();

            // Compact sizing
            var isCompactEnabled = Mvx.IoCProvider.Resolve<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.IsCompactSizing));
            if (isCompactEnabled)
            {
                Application.Current.Resources.MergedDictionaries.Add(
                   new ResourceDictionary { Source = new Uri(@"ms-appx:///Microsoft.UI.Xaml/DensityStyles/Compact.xaml", UriKind.Absolute) });
            }

            // Analytics
            var disableAnalytics = Mvx.IoCProvider.Resolve<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.DisableAnalytics));
            if (!disableAnalytics)
            {
                // Initialize AppCenter
                //AppCenter.Start("f2193544-6a38-42f6-92bd-69964bc3a0e8", typeof(Analytics), typeof(Crashes));
            }

            var theme = Mvx.IoCProvider.Resolve<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ElementTheme));
            Enum.TryParse(theme, out Theme elementTheme);
            await Mvx.IoCProvider.Resolve<IInteropService>().SetThemeAsync(elementTheme);

            Mvx.IoCProvider.Resolve<AlbumArtService>().Initialize();
        }
    }


    public sealed partial class App
    {
        public App()
        {
            InitializeComponent();

        }
    }
}
