using System;
using System.Collections.Generic;
using System.Linq;
using Strings = Stylophone.Localization.Strings.Resources;
using Stylophone.Common.ViewModels;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using MpcNET.Commands.Playback;
using UIKit;
using Foundation;
using Stylophone.iOS.ViewControllers;
using Stylophone.iOS.Services;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Stylophone.iOS.ViewModels
{
    public class ShellViewModel : ShellViewModelBase
    {
      
        private UICollectionViewDiffableDataSource<NSString, NavigationSidebarItem> _sidebarDataSource;
        private UICollectionView _collectionView;

        public ShellViewModel(INavigationService navigationService, INotificationService notificationService, IDispatcherService dispatcherService, MPDConnectionService mpdService):
            base(navigationService, notificationService, dispatcherService, mpdService)
        {
        }

        internal void Initialize(UICollectionView collectionView, UICollectionViewDiffableDataSource<NSString, NavigationSidebarItem> sidebarDataSource)
        {
            _sidebarDataSource = sidebarDataSource;
            _collectionView = collectionView;

            var concreteNavService = _navigationService as NavigationService;
            concreteNavService.Navigated += UpdateNavigationViewSelection;

            PropertyChanged += UpdateDatabaseIndicator;
        }

        private void UpdateDatabaseIndicator(object sender, PropertyChangedEventArgs e)
        {
            // Only run this code when IsServerUpdating changes
            if (e.PropertyName != nameof(IsServerUpdating)) return;

            var snapshot = new NSDiffableDataSourceSectionSnapshot<NavigationSidebarItem>();
            var item = NavigationSidebarItem.GetRow(Strings.DatabaseUpdateHeader, null, null, UIImage.GetSystemImage("hourglass"));

            if (IsServerUpdating)
                snapshot.AppendItems(new[] { item });

            _sidebarDataSource.ApplySnapshot(snapshot, new NSString("database_update"), false);
        }

        private void UpdateNavigationViewSelection(object sender, CoreNavigationEventArgs e)
        {
            var selectedItem = _collectionView.GetIndexPathsForSelectedItems().FirstOrDefault();
            if (selectedItem != null)
                _collectionView.DeselectItem(selectedItem, true);

            var navItems = _sidebarDataSource.Snapshot.GetItemIdentifiersInSection(new NSString("base"));
            var navItem = navItems.Where(item => item.Target == e.NavigationTarget).FirstOrDefault();

            if (navItem != null)
                _collectionView.SelectItem(_sidebarDataSource.GetIndexPath(navItem), true, UICollectionViewScrollPosition.None);
        }

        protected override void UpdatePlaylistNavigation()
        {
            // Update the datasource for the sidebar's playlists.
            var playlists = _mpdService.Playlists;

            var snapshot = new NSDiffableDataSourceSectionSnapshot<NavigationSidebarItem>();
            var header = NavigationSidebarItem.GetExpandableRow(Strings.PlaylistsHeader);

            var items = new List<NavigationSidebarItem>();

            foreach (var playlist in playlists)
            {
                var item = NavigationSidebarItem.GetRow(playlist.Name, typeof(PlaylistViewModel), null,
                    UIImage.GetSystemImage("music.note.list"));
                items.Add(item);
            }

            snapshot.AppendItems(new[] { header });
            snapshot.AppendItems(items.ToArray(), header);

            _sidebarDataSource.ApplySnapshot(snapshot, new NSString("playlists"), false);
        }

        protected override void Loaded()
        {

        }

        protected override void Navigate(object itemInvoked)
        {
            if (itemInvoked is string s)
            {
                Navigate(typeof(SettingsViewModel));
                return;
            }

            if (itemInvoked is NavigationSidebarItem sidebarItem)
            {
                if (sidebarItem.Command == nameof(AddRandomTracksCommand))
                {
                    AddRandomTracksCommand.Execute(null);
                    return;
                }
                else
                {
                    var pageType = sidebarItem.Target;

                    // Playlist items navigate with their name as parameter
                    if (pageType == typeof(PlaylistViewModel))
                        Navigate(pageType, sidebarItem.Title);
                    else
                        Navigate(pageType);
                }
            }
        }

        private void Navigate(Type viewModel, object parameter = null)
        {
            // On phones, since the sidebar is its own screen,
            // we don't want to keep the root view controller(usually the queue) when navigating from the sidebar.
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
                ((NavigationService)_navigationService).NavigationController.SetViewControllers(new UIViewController[0], false);

            _navigationService.Navigate(viewModel, parameter);
        }
    }
}
