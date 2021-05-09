using System;
using System.Collections.Generic;
using System.Linq;

using Stylophone.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Stylophone.Common.ViewModels;
using Windows.System;
using Stylophone.Common.Interfaces;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using WinUI = Microsoft.UI.Xaml.Controls;
using Stylophone.Common.Services;
using Windows.Foundation;
using MpcNET.Commands.Playback;
using MvvmCross.Navigation.EventArguments;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using MvvmCross;
using Stylophone.Services;

namespace Stylophone.ViewModels
{
    public class ShellViewModel : ShellViewModelBase
    {
        private IList<KeyboardAccelerator> _keyboardAccelerators;
        private KeyboardAccelerator _altLeftKeyboardAccelerator;
        private KeyboardAccelerator _backKeyboardAccelerator;
        private KeyboardAccelerator _spaceKeyboardAccelerator;
        
        private WinUI.NavigationView _navigationView;
        private WinUI.NavigationViewItem _playlistContainer;
        private InAppNotification _notificationHolder;

        private Frame _contentFrame;

        public ShellViewModel(IMvxNavigationService navigationService, INotificationService notificationService, IDialogService dialogService, IDispatcherService dispatcherService, MPDConnectionService mpdService):
            base(navigationService, notificationService, dialogService, dispatcherService, mpdService)
        {
        }

        public void Initialize(Frame shellFrame, WinUI.NavigationView navigationView, WinUI.NavigationViewItem playlistContainer, InAppNotification notificationHolder, IList<KeyboardAccelerator> keyboardAccelerators)
        {
            _contentFrame = shellFrame;
            _navigationView = navigationView;
            _playlistContainer = playlistContainer;
            _notificationHolder = notificationHolder;
            _keyboardAccelerators = keyboardAccelerators;

            _navigationService.AfterClose += UpdateNavigationViewSelection;
            _navigationService.AfterNavigate += UpdateNavigationViewSelection;

            _navigationView.BackRequested += OnBackRequested;
            _navigationView.AutoSuggestBox.TextChanged += UpdateSearchSuggestions;
            _navigationView.AutoSuggestBox.QuerySubmitted += HandleSearchRequest;

            _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, GoBack, VirtualKeyModifiers.Menu);
            _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack, GoBack);
            _spaceKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Space, PauseOrPlay);
        }

        internal Frame GetShellFrame() => _contentFrame;

        private void UpdateNavigationViewSelection(object sender, IMvxNavigateEventArgs e)
        {
            if (e.ViewModel is ShellViewModel) return;

            if (e.ViewModel is SettingsViewModel)
            {
                _navigationView.SelectedItem = _navigationView.SettingsItem as WinUI.NavigationViewItem;
                return;
            }

            if (e.ViewModel is PlaylistViewModel)
            {
                _navigationView.SelectedItem = _playlistContainer;
                return;
            }

            _navigationView.SelectedItem = _navigationView.MenuItems
                           .OfType<WinUI.NavigationViewItem>()
                           .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e.ViewModel.GetType()));
        }

        private bool IsMenuItemForPageType(WinUI.NavigationViewItem menuItem, Type sourcePageType)
        {
            var pageType = menuItem.GetValue(NavHelper.NavigateToProperty) as Type;
            return pageType == sourcePageType;
        }

        protected override void ShowInAppNotification(object sender, InAppNotificationRequestedEventArgs e)
        {
            _dispatcherService.ExecuteOnUIThreadAsync(() => _notificationHolder.Show(e.NotificationText, e.NotificationTime));
        }

        protected override void UpdatePlaylistNavigation()
        {
            // Update the navigationview by hand - It ain't clean but partial databinding would be an even bigger mess...
            var playlists = _mpdService.Playlists;

            // Remove all menuitems in the "Playlists" menu
            _playlistContainer.MenuItems.Clear();

            foreach (var playlist in playlists)
            {
                var navigationViewItem = new WinUI.NavigationViewItem();
                navigationViewItem.Icon = new SymbolIcon(Symbol.MusicInfo);
                navigationViewItem.Content = playlist.Name;
                NavHelper.SetNavigateTo(navigationViewItem, typeof(PlaylistViewModel));
                _playlistContainer.MenuItems.Add(navigationViewItem);
            }
        }

        protected override void OnLoaded()
        {
            // Keyboard accelerators are added here to avoid showing 'Alt + left' tooltip on the page.
            // More info on tracking issue https://github.com/Microsoft/microsoft-ui-xaml/issues/8
            _keyboardAccelerators.Add(_altLeftKeyboardAccelerator);
            _keyboardAccelerators.Add(_backKeyboardAccelerator);
            _keyboardAccelerators.Add(_spaceKeyboardAccelerator);
        }

        protected override void OnItemInvoked(object args)
        {
            var navArgs = (WinUI.NavigationViewItemInvokedEventArgs)args;

            if (navArgs.IsSettingsInvoked)
            {
                _navigationService.Navigate<SettingsViewModel>();
                return;
            }

            // Slight trick of hand to avoid triggering the navigationService on items that don't have a matching page.
            // This is done through setting SelectsOnInvoked in XAML.
            if (navArgs.InvokedItemContainer is WinUI.NavigationViewItem i && !i.SelectsOnInvoked)
                return;

            var item = _navigationView.MenuItems.Union(_playlistContainer.MenuItems)
                            .OfType<WinUI.NavigationViewItem>()
                            .FirstOrDefault(menuItem => (string)menuItem.Content == (string)navArgs.InvokedItem);

            if (item == null)
                return;

            var pageType = item.GetValue(NavHelper.NavigateToProperty) as Type;

            // Playlist items navigate with their name as parameter
            if (_playlistContainer.MenuItems.Contains(item))
                _navigationService.Navigate(pageType, item.Content);
            else
                _navigationService.Navigate(pageType);

            //IsBackEnabled = await _navigationService.CanNavigate("..");
        }

        private void OnBackRequested(WinUI.NavigationView sender, WinUI.NavigationViewBackRequestedEventArgs args)
        {
            // Close the currently shown VM
            _navigationService.Close(_contentFrame.DataContext as IMvxViewModel);
        }

        private async void UpdateSearchSuggestions(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
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
        }

        private KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, TypedEventHandler<KeyboardAccelerator, KeyboardAcceleratorInvokedEventArgs> onInvoked, VirtualKeyModifiers? modifiers = null)
        {
            var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
            if (modifiers.HasValue)
            {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += onInvoked;
            return keyboardAccelerator;
        }

        private async void GoBack(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var result = _navigationService.Close(_contentFrame.DataContext as IMvxViewModel);
            args.Handled = await result;
        }

        private async void PauseOrPlay(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            await _mpdService.SafelySendCommandAsync(new PauseResumeCommand());
            args.Handled = true;
        }
    }
}
