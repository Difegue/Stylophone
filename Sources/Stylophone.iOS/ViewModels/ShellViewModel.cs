﻿using System;
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
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System.Threading.Tasks;

namespace Stylophone.iOS.ViewModels
{
    public class ShellViewModel : ShellViewModelBase
    {
      
        private UICollectionViewDiffableDataSource<NSString, NavigationSidebarItem> _sidebarDataSource;
        //private InAppNotification _notificationHolder;

        public ShellViewModel(INavigationService navigationService, INotificationService notificationService, IDispatcherService dispatcherService, MPDConnectionService mpdService):
            base(navigationService, notificationService, dispatcherService, mpdService)
        {
        }

        internal void Initialize(UICollectionView collectionView, UICollectionViewDiffableDataSource<NSString, NavigationSidebarItem> sidebarDataSource)
        {
            _sidebarDataSource = sidebarDataSource;

            var concreteNavService = _navigationService as NavigationService;
            concreteNavService.InitializeHeaderBinding(this);
        }

        protected override void ShowInAppNotification(object sender, InAppNotificationRequestedEventArgs e)
        {
            // TODO
            //_dispatcherService.ExecuteOnUIThreadAsync(() => _notificationHolder.Show(e.NotificationText, e.NotificationTime));
        }

        protected override void UpdatePlaylistNavigation()
        {
            // Update the datasource for the sidebar's playlists.
            var playlists = _mpdService.Playlists;

            var snapshot = new NSDiffableDataSourceSectionSnapshot<NavigationSidebarItem>();
            var header = NavigationSidebarItem.GetExpandableRow(Strings.AddedToPlaylistText); //TODO

            var items = new List<NavigationSidebarItem>();

            foreach (var playlist in playlists)
            {
                var item = NavigationSidebarItem.GetRow(playlist.Name, typeof(PlaylistViewModel), null,
                    UIImage.GetSystemImage("music.note.list"));
                items.Add(item);
            }

            snapshot.AppendItems(new[] { header });
            snapshot.ExpandItems(new[] { header });
            snapshot.AppendItems(items.ToArray(), header);

            _sidebarDataSource.ApplySnapshot(snapshot, new NSString("playlists"), false);
        }

        protected override void OnLoaded()
        {

        }

        protected override void OnItemInvoked(object itemInvoked)
        {
            //var navArgs = (WinUI.NavigationViewItemInvokedEventArgs)args;

            if (itemInvoked is string s)
            {
                _navigationService.Navigate<SettingsViewModel>();
                return;
            }

            if (itemInvoked is NavigationSidebarItem sidebarItem)
            {
                var pageType = sidebarItem.Target;

                // Playlist items navigate with their name as parameter
                if (pageType == typeof(PlaylistViewModel))
                    _navigationService.Navigate(pageType, sidebarItem.Title);
                else
                    _navigationService.Navigate(pageType);
            }
        }

        /*
         * private async void UpdateSearchSuggestions(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
                return;

            // Clear out suggestions before filling them up again, as it takes a bit of time.
            sender.ItemsSource = new List<object>();

            sender.ItemsSource = await SearchAsync(sender.Text);
        }

        private async void HandleSearchRequest(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            await HandleSearchRequestAsync(sender.Text, args.ChosenSuggestion);
        }*/
    }
}
