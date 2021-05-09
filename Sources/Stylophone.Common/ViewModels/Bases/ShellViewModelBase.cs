using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

using MpcNET.Commands.Database;
using MpcNET.Commands.Queue;
using MpcNET.Tags;
using MpcNET.Types;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Navigation.EventArguments;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.ViewModels
{
    public abstract class ShellViewModelBase : ViewModelBase
    {
        protected IMvxNavigationService _navigationService;
        protected INotificationService _notificationService;
        protected MPDConnectionService _mpdService;

        public ShellViewModelBase(IMvxNavigationService navigationService, INotificationService notificationService, IDialogService dialogService, IDispatcherService dispatcherService, MPDConnectionService mpdService):
            base(dispatcherService)
        {
            _navigationService = navigationService;
            _notificationService = notificationService;
            _mpdService = mpdService;

            // First View, use that to initialize our DispatcherService
            _dispatcherService.Initialize();

            Task.Run(async () =>
            {
                await dialogService.ShowFirstRunDialogIfAppropriateAsync();

                var host = Mvx.IoCProvider.Resolve<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ServerHost));
                var port = Mvx.IoCProvider.Resolve<IApplicationStorageService>().GetValue<int>(nameof(SettingsViewModel.ServerPort));

                mpdService.SetServerInfo(host, port);
                await mpdService.InitializeAsync(true);
            });

            ((NotificationServiceBase)_notificationService).InAppNotificationRequested += ShowInAppNotification;

            _navigationService.AfterClose += OnFrameNavigated;
            _navigationService.AfterNavigate += OnFrameNavigated;

            _mpdService.PlaylistsChanged += (s, e) => TryUpdatePlaylists();
        }

        private string _shellHeader;
        public string HeaderText
        {
            get { return _shellHeader; }
            set { Set(ref _shellHeader, value); }
        }

        private ICommand _loadedCommand;
        public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new MvxCommand(OnLoaded));

        private ICommand _navigateCommand;
        public ICommand NavigateCommand => _navigateCommand ?? (_navigateCommand = new MvxCommand<object>(OnItemInvoked));

        protected abstract void ShowInAppNotification(object sender, InAppNotificationRequestedEventArgs e);
        protected abstract void OnLoaded();
        protected abstract void OnItemInvoked(object item);
        protected abstract void UpdatePlaylistNavigation();

        private void OnFrameNavigated(object sender, IMvxNavigateEventArgs e)
        {
            if (e.ViewModel is ShellViewModelBase) return;

            var viewModelType = e.ViewModel.GetType();

            // Use some reflection magic to get the static Header text for this ViewModel
            var headerMethod = viewModelType.GetMethod(nameof(ViewModelBase.GetHeader), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            HeaderText = (string)headerMethod?.Invoke(null, null) ?? "";
        }

        public async Task<IList<object>> SearchAsync(string text)
        {
            var suitableItems = new List<object>();

            if (text.Trim().Length > 0)
                suitableItems.Add(string.Format(Resources.GoToDetailSearch, text));

            if (text.Length > 2)
            {
                var response = await _mpdService.SafelySendCommandAsync(new SearchCommand(FindTags.Title, text));

                if (response != null)
                {
                    foreach (var f in response)
                        suitableItems.Add(f);
                }
            }

            return suitableItems;
        }

        public async Task HandleSearchRequestAsync(string text, object chosenSuggestion)
        {
            if (chosenSuggestion != null && chosenSuggestion is IMpdFile)
            {
                var response = await _mpdService.SafelySendCommandAsync(new AddCommand((chosenSuggestion as IMpdFile).Path));

                if (response != null)
                    _notificationService.ShowInAppNotification(Resources.AddedToQueueText);
            }
            else
            {
                // Navigate to detailed search page
                await _navigationService.Navigate<SearchResultsViewModel,string>(text);
                HeaderText = string.Format(Resources.SearchResultsFor, text);
            }
        }

        private void TryUpdatePlaylists()
        {
            _dispatcherService.ExecuteOnUIThreadAsync(() => {
                try
                {
                    UpdatePlaylistNavigation();
                }
                catch (Exception e)
                {
                    //TODO localize
                    _notificationService.ShowInAppNotification($"Updating Playlist Navigation failed: {e.Message}", false);
                }
            });
        }
    }
}
