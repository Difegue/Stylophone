﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using FluentMPC.Helpers;
using FluentMPC.Services;
using FluentMPC.Views;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using MpcNET.Commands.Database;
using MpcNET.Commands.Queue;
using MpcNET.Tags;
using MpcNET.Types;
using Windows.System;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using WinUI = Microsoft.UI.Xaml.Controls;

namespace FluentMPC.ViewModels
{
    public class ShellViewModel : Observable
    {
        private readonly KeyboardAccelerator _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);
        private readonly KeyboardAccelerator _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);

        private bool _isBackEnabled;
        private Thickness _frameMargin;
        private IList<KeyboardAccelerator> _keyboardAccelerators;
        private WinUI.NavigationView _navigationView;
        private WinUI.NavigationViewItem _selected;
        private WinUI.NavigationViewItem _playlistContainer;
        private InAppNotification _notificationHolder;
        private ICommand _loadedCommand;
        private ICommand _itemInvokedCommand;

        public bool IsBackEnabled
        {
            get { return _isBackEnabled; }
            set { Set(ref _isBackEnabled, value); }
        }

        public WinUI.NavigationViewItem Selected
        {
            get { return _selected; }
            set { Set(ref _selected, value); }
        }

        public Thickness FrameMargin
        {
            get { return _frameMargin; }
            set { Set(ref _frameMargin, value); }
        }

        public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));

        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<WinUI.NavigationViewItemInvokedEventArgs>(OnItemInvoked));

        public ShellViewModel()
        {
        }

        public void Initialize(Frame frame, WinUI.NavigationView navigationView, WinUI.NavigationViewItem playlistContainer, InAppNotification notificationHolder, IList<KeyboardAccelerator> keyboardAccelerators)
        {
            _navigationView = navigationView;
            _playlistContainer = playlistContainer;
            _notificationHolder = notificationHolder;
            _keyboardAccelerators = keyboardAccelerators;
            NavigationService.Frame = frame;
            NavigationService.NavigationFailed += Frame_NavigationFailed;
            NavigationService.Navigated += Frame_Navigated;
            _navigationView.BackRequested += OnBackRequested;

            _navigationView.AutoSuggestBox.TextChanged += UpdateSearchSuggestions;
            _navigationView.AutoSuggestBox.QuerySubmitted += HandleSearchRequest;

            NotificationService.InAppNotificationRequested += Show_InAppNotification;

            DispatcherService.ExecuteOnUIThreadAsync(() => UpdatePlaylistNavigation());
            MPDConnectionService.PlaylistsChanged += (s,e) =>
                 DispatcherService.ExecuteOnUIThreadAsync(() => UpdatePlaylistNavigation());
        }

        private void Show_InAppNotification(object sender, InAppNotificationRequestedEventArgs e)
        {
            DispatcherService.ExecuteOnUIThreadAsync(() => _notificationHolder.Show(e.NotificationText, e.NotificationTime));
        }

        private async void OnLoaded()
        {
            // Keyboard accelerators are added here to avoid showing 'Alt + left' tooltip on the page.
            // More info on tracking issue https://github.com/Microsoft/microsoft-ui-xaml/issues/8
            _keyboardAccelerators.Add(_altLeftKeyboardAccelerator);
            _keyboardAccelerators.Add(_backKeyboardAccelerator);
            await Task.CompletedTask;
        }

        private void OnItemInvoked(WinUI.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                NavigationService.Navigate(typeof(SettingsPage));
                return;
            }

            // Slight trick of hand to avoid triggering the navigationService on items that don't have a matching page.
            // This is done through setting SelectsOnInvoked in XAML.
            if (args.InvokedItemContainer is WinUI.NavigationViewItem i && !i.SelectsOnInvoked)
                return;

            var item = _navigationView.MenuItems.Union(_playlistContainer.MenuItems)
                            .OfType<WinUI.NavigationViewItem>()
                            .FirstOrDefault(menuItem => (string)menuItem.Content == (string)args.InvokedItem);

            if (item == null)
                return;

            var pageType = item.GetValue(NavHelper.NavigateToProperty) as Type;

            // Playlist items navigate with their name as parameter
            if (_playlistContainer.MenuItems.Contains(item))
                NavigationService.Navigate(pageType, item.Content);
            else
                NavigationService.Navigate(pageType);
        }

        private void OnBackRequested(WinUI.NavigationView sender, WinUI.NavigationViewBackRequestedEventArgs args)
        {
            NavigationService.GoBack();
        }

        private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw e.Exception;
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            IsBackEnabled = NavigationService.CanGoBack;

            if (e.SourcePageType == typeof(LibraryDetailPage) || e.SourcePageType == typeof(PlaylistPage) || e.SourcePageType == typeof(PlaybackView))
            {
                // Special margin to extend frame to the titlebar
                FrameMargin = new Thickness(0, -32, 0, 0);
            }
            else
            {
                FrameMargin = new Thickness(0, 0, 0, 0);
            }

            if (e.SourcePageType == typeof(SettingsPage))
            {
                Selected = _navigationView.SettingsItem as WinUI.NavigationViewItem;
                return;
            }

            // Create a transient navigationviewitem to show a custom header value.
            if (e.SourcePageType == typeof(SearchResultsPage))
            {
                var item = new WinUI.NavigationViewItem();
                item.Content = string.Format("SearchResultsFor".GetLocalized(), e.Parameter as string);
                Selected = item;
                return;
            }

            Selected = _navigationView.MenuItems
                            .OfType<WinUI.NavigationViewItem>()
                            .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e.SourcePageType));
        }

        private bool IsMenuItemForPageType(WinUI.NavigationViewItem menuItem, Type sourcePageType)
        {
            var pageType = menuItem.GetValue(NavHelper.NavigateToProperty) as Type;
            return pageType == sourcePageType;
        }

        private void UpdatePlaylistNavigation()
        {
            // Update the navigationview by hand - It ain't clean but partial databinding would be an even bigger mess...
            var playlists = MPDConnectionService.Playlists;

            // Remove all menuitems in the "Playlists" menu
            _playlistContainer.MenuItems.Clear();

            try
            {
                foreach (var playlist in playlists)
                {
                    var navigationViewItem = new WinUI.NavigationViewItem();
                    navigationViewItem.Icon = new SymbolIcon(Symbol.MusicInfo);
                    navigationViewItem.Content = playlist.Name;
                    NavHelper.SetNavigateTo(navigationViewItem, typeof(PlaylistPage));
                    _playlistContainer.MenuItems.Add(navigationViewItem);
                }
            } catch (Exception e)
            {
                NotificationService.ShowInAppNotification($"Updating Playlist Navigation failed: {e.Message}", 0);
            }
        }

        private async void UpdateSearchSuggestions(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
                return;

            var suitableItems = new List<object>();

            if (sender.Text.Trim().Length > 0)
                suitableItems.Add(string.Format("GoToDetailSearch".GetLocalized(), sender.Text));

            if (sender.Text.Length > 2)
            {
                // Clear out suggestions before filling them up again, as it takes a bit of time.
                sender.ItemsSource = suitableItems;
                var response = await MPDConnectionService.SafelySendCommandAsync(new SearchCommand(FindTags.Title, sender.Text));

                if (response != null)
                {
                    foreach (var f in response)
                    {
                        suitableItems.Add(f);
                    }
                }
            }

            sender.ItemsSource = suitableItems;
        }

        private async void HandleSearchRequest(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null && args.ChosenSuggestion is IMpdFile)
            {
                var response = await MPDConnectionService.SafelySendCommandAsync(new AddCommand((args.ChosenSuggestion as IMpdFile).Path));

                if (response != null)
                    NotificationService.ShowInAppNotification("AddedToQueueText".GetLocalized());
            }
            else
            {
                // Navigate to detailed search page
                NavigationService.Navigate(typeof(SearchResultsPage), args.QueryText);
            }
        }


        private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
        {
            var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
            if (modifiers.HasValue)
            {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
            return keyboardAccelerator;
        }

        private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var result = NavigationService.GoBack();
            args.Handled = result;
        }

    }
}
