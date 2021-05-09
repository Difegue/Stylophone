using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Stylophone.Common
{
    public class App : MvxApplication
    {
        // This class does mostly nothing. All IoC and initialization is done on a by-platform basis for now.

        public override void Initialize()
        {
            // Common Services
            Mvx.IoCProvider.RegisterSingleton(() => Mvx.IoCProvider.IoCConstruct<MPDConnectionService>());
            Mvx.IoCProvider.RegisterSingleton(() => Mvx.IoCProvider.IoCConstruct<AlbumArtService>());

            // Singleton ViewModels
            Mvx.IoCProvider.RegisterSingleton(() => Mvx.IoCProvider.IoCConstruct<AlbumDetailViewModel>());
            Mvx.IoCProvider.RegisterSingleton(() => Mvx.IoCProvider.IoCConstruct<FoldersViewModel>());
            Mvx.IoCProvider.RegisterSingleton(() => Mvx.IoCProvider.IoCConstruct<PlaylistViewModel>());
            Mvx.IoCProvider.RegisterSingleton(() => Mvx.IoCProvider.IoCConstruct<QueueViewModel>());
            Mvx.IoCProvider.RegisterSingleton(() => Mvx.IoCProvider.IoCConstruct<SearchResultsViewModel>());
            Mvx.IoCProvider.RegisterSingleton(() => Mvx.IoCProvider.IoCConstruct<SettingsViewModel>());

            RegisterCustomAppStart<AppStart>();
        }
    }


    public class AppStart : MvxAppStart
    {
        private readonly IInteropService _interopService;

        public AppStart(IMvxApplication application, IMvxNavigationService navigationService, IInteropService interop) : base(application, navigationService)
        {
            _interopService = interop;
        }

        protected override Task NavigateToFirstViewModel(object hint = null)
        {
            Type viewModelType = _interopService.GetInitialViewModelType();
            return NavigationService.Navigate(viewModelType);
        }
    }
}
