using Foundation;
using Stylophone.Common.Interfaces;
using Stylophone.Common.ViewModels;
using Stylophone.iOS.ViewControllers;
using Stylophone.iOS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace Stylophone.iOS.Services
{
    public class NavigationService : NavigationServiceBase
    {
        private Dictionary<Type, UIStoryboard> _viewModelToStoryboardDictionary = new Dictionary<Type, UIStoryboard>()
        {
            { typeof(QueueViewModel), UIStoryboard.FromName("Queue", null) },
            { typeof(SettingsViewModel), UIStoryboard.FromName("Settings", null)},
            { typeof(AlbumDetailViewModel), UIStoryboard.FromName("AlbumDetail", null) },
            { typeof(PlaybackViewModelBase), UIStoryboard.FromName("NowPlaying", null) },
            //{ typeof(SearchResultsViewModel), typeof(SearchResultsPage) },
            //{ typeof(FoldersViewModel), typeof(FoldersPage) },
            //{ typeof(PlaylistViewModel), typeof(PlaylistPage) },
            //{ typeof(LibraryViewModel), typeof(LibraryPage) }
        };

        public NavigationService()
        {

        }

        public override bool CanGoBack => NavigationController.ViewControllers?.Count() > 1;

        public override Type CurrentPageViewModelType => _viewModelToStoryboardDictionary.Keys.Where(
            k => _viewModelToStoryboardDictionary[k] == NavigationController.VisibleViewController.Storyboard).FirstOrDefault();

        private object _lastParamUsed;
        public override void NavigateImplementation(Type viewmodelType, object parameter = null)
        {
            if (viewmodelType == null) return;

            // Get the matching page and navigate to it
            var storyboard = _viewModelToStoryboardDictionary.GetValueOrDefault(viewmodelType); 

            // Don't open the same page multiple times
            if (NavigationController.VisibleViewController?.Storyboard != storyboard || (parameter != null && !parameter.Equals(_lastParamUsed)))
            {
                UIViewController viewController;
                var existingViewControllers = NavigationController.ViewControllers.Where(vc => vc.Storyboard == storyboard).ToList();

                // If the VC is already in the stack, reuse it
                if (existingViewControllers.Count > 0)
                {
                    viewController = existingViewControllers.First();
                    (viewController as IPreparableViewController)?.Prepare(parameter);
                    NavigationController.PopToViewController(viewController, true);
                }
                else
                {
                    viewController = storyboard.InstantiateInitialViewController();
                    (viewController as IPreparableViewController)?.Prepare(parameter);
                    NavigationController.PushViewController(viewController, true);
                }

                _lastParamUsed = parameter;
            }
        }

        public override void SetListDataItemForNextConnectedAnimation(object item)
        {
            // TODO
        }

        public override bool GoBackImplementation()
        {
            if (CanGoBack)
            {
                NavigationController.PopViewController(true);
                return true;
            }

            return false;
        }

        private UINavigationController _navController;
        public UINavigationController NavigationController
        {
            get
            {
                return _navController;
            }

            set
            {
                _navController = value;
            }
        }

    }
}
