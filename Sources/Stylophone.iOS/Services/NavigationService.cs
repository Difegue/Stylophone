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
            { typeof(SearchResultsViewModel), UIStoryboard.FromName("SearchResults", null) },
            { typeof(FoldersViewModel), UIStoryboard.FromName("Folders", null) },
            { typeof(PlaylistViewModel), UIStoryboard.FromName("Playlist", null) },
            { typeof(LibraryViewModel), UIStoryboard.FromName("Library", null) }
        };

        public NavigationService()
        {
            _viewControllers = new List<UIViewController>();
        }

        public override bool CanGoBack => NavigationController.ViewControllers?.Length > 1;

        public override Type CurrentPageViewModelType => _viewModelToStoryboardDictionary.Keys.Where(
            k => _viewModelToStoryboardDictionary[k] == NavigationController.VisibleViewController.Storyboard).FirstOrDefault();

        private List<UIViewController> _viewControllers;

        internal void AddViewControllerToNavigationStack(UIViewController viewController)
        {
            _viewControllers.Add(viewController);
        }

        public UIStoryboard GetStoryboardForViewModel(Type viewmodelType)
        {
           return _viewModelToStoryboardDictionary.GetValueOrDefault(viewmodelType);
        }

        private object _lastParamUsed;
        public override void NavigateImplementation(Type viewmodelType, object parameter = null)
        {
            if (viewmodelType == null) return;

            // Get the matching page and navigate to it
            var storyboard = GetStoryboardForViewModel(viewmodelType);

            // Don't open the same page multiple times
            if (NavigationController.VisibleViewController?.Storyboard != storyboard || (parameter != null && !parameter.Equals(_lastParamUsed)))
            {
                UIViewController viewController;
                var viewControllerInStack = NavigationController.ViewControllers.Where(vc => vc.Storyboard == storyboard).ToList();
                var viewControllerLoaded = _viewControllers.Where(vc => vc.Storyboard == storyboard).ToList();

                // If the VC is already in the stack, reuse it
                if (viewControllerInStack.Count > 0)
                {
                    viewController = viewControllerInStack.First();
                    (viewController as IPreparableViewController)?.Prepare(parameter);
                    NavigationController.PopToViewController(viewController, true);
                }
                else // If we already loaded this VC, push it onto the NavigationController again
                if (viewControllerLoaded.Count > 0)
                {
                    viewController = viewControllerLoaded.First();
                    (viewController as IPreparableViewController)?.Prepare(parameter);

                    if (NavigationController.ViewControllers.Length == 0)
                        NavigationController.ViewControllers = new UIViewController[] { viewController };
                    else
                        NavigationController.PushViewController(viewController, true);
                }
                else // This is truly new, load the VC from scratch
                {
                    viewController = storyboard.InstantiateInitialViewController();
                    (viewController as IPreparableViewController)?.Prepare(parameter);
                    _viewControllers.Add(viewController);

                    if (NavigationController.ViewControllers.Length == 0)
                        NavigationController.ViewControllers = new UIViewController[] { viewController };
                    else
                        NavigationController.PushViewController(viewController, true);
                }

                _lastParamUsed = parameter;
            }

            // Make sure the detail view is showing, even if we didn't navigate to a new VC
            (UIApplication.SharedApplication.Delegate as AppDelegate).ShowDetailView();
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
