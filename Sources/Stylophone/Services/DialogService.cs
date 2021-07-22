using System;
using System.Threading.Tasks;

using Stylophone.Views;
using Microsoft.Toolkit.Uwp.Helpers;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stylophone.Services
{
    public class DialogService: IDialogService
    {
        private IDispatcherService _dispatcherService;
        private IApplicationStorageService _storageService;
        private INavigationService _navigationService;
        private MPDConnectionService _mpdService;

        public DialogService(IDispatcherService dispatcherService, INavigationService navigationService, IApplicationStorageService storageService, MPDConnectionService mpdService)
        {
            _dispatcherService = dispatcherService;
            _navigationService = navigationService;
            _storageService = storageService;
            _mpdService = mpdService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowExistingPlaylists">If set to FALSE, the dialog will only allow you to create a new playlist.</param>
        /// <returns></returns>
        public async Task<string> ShowAddToPlaylistDialog(bool allowExistingPlaylists = true)
        {
            var dialog = new AddToPlaylistDialog(_mpdService, allowExistingPlaylists);
            var result = await _dispatcherService.EnqueueAsync(async () => await dialog.ShowAsync());

            // Return new playlist name if checked, selected playlist otherwise
            return result == ContentDialogResult.Primary ? dialog.AddNewPlaylist ? dialog.PlaylistName : dialog.SelectedPlaylist : null;
        }

        private bool shown = false;
        public async Task ShowFirstRunDialogIfAppropriateAsync()
        {
            await _dispatcherService.ExecuteOnUIThreadAsync(async () =>
                {
                    if (SystemInformation.Instance.IsFirstRun && !shown)
                    {
                        shown = true;
                        var dialog = new FirstRunDialog();
                        await dialog.ShowAsync();
                        _navigationService.Navigate<SettingsViewModel>();
                    }
                });
        }

        public async Task<bool> ShowConfirmDialogAsync(string title, string text, string primaryButtonText = null, string cancelButtonText = null)
        {
            ContentDialog confirmDialog = new ContentDialog
            {
                Title = title,
                Content = text,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = cancelButtonText
            };

            var theme = _storageService.GetValue<string>(nameof(SettingsViewModel.ElementTheme));
            Enum.TryParse(theme, out ElementTheme elementTheme); // We can parse the enum directly to ElementTheme since the names are the same!

            confirmDialog.RequestedTheme = elementTheme;

            ContentDialogResult result = await confirmDialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }
    }
}
