using System;
using System.Threading.Tasks;

using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Strings = Stylophone.Localization.Strings.Resources;
using UIKit;

namespace Stylophone.iOS.Services
{
    public class DialogService: IDialogService
    {
        private IDispatcherService _dispatcherService;
        private IApplicationStorageService _storageService;
        private MPDConnectionService _mpdService;

        public DialogService(IDispatcherService dispatcherService, IApplicationStorageService storageService, MPDConnectionService mpdService)
        {
            _dispatcherService = dispatcherService;
            _storageService = storageService;
            _mpdService = mpdService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowExistingPlaylists">If set to FALSE, the dialog will only allow you to create a new playlist.</param>
        /// <returns></returns>
        public Task<string> ShowAddToPlaylistDialog(bool allowExistingPlaylists = true)
        {
            //var dialog = new AddToPlaylistDialog(_mpdService, allowExistingPlaylists);
            //var result = await _dispatcherService.EnqueueAsync(async () => await dialog.ShowAsync());

            // Return new playlist name if checked, selected playlist otherwise
            //return result == ContentDialogResult.Primary ? dialog.AddNewPlaylist ? dialog.PlaylistName : dialog.SelectedPlaylist : null;
            return null;
        }

        private bool shown = false;
        public async Task ShowFirstRunDialogIfAppropriateAsync()
        {
            await _dispatcherService.ExecuteOnUIThreadAsync(async () =>
                {
                    if (!_storageService.GetValue<bool>("HasLaunchedOnce") && !shown)
                    {
                        shown = true;
                        _storageService.SetValue<bool>("HasLaunchedOnce", true);

                        // TODO
                        await ShowConfirmDialogAsync(Strings.ExceptionSettingsStorageExtensionsFileNameIsNullOrEmpty, Strings.ErrorGeneric, Strings.OKButtonText);
                    }
                });
        }

        public async Task<bool> ShowConfirmDialogAsync(string title, string text, string primaryButtonText = null, string cancelButtonText = null)
        {
            var confirmDialog = UIAlertController.Create(title, text, UIAlertControllerStyle.Alert);
            var tcs = new TaskCompletionSource<bool>();

            if (primaryButtonText != null)
            {
                var primaryAction = UIAlertAction.Create(primaryButtonText, UIAlertActionStyle.Default, (act) => { tcs.SetResult(true); });
                confirmDialog.AddAction(primaryAction);
            }

            if (cancelButtonText != null)
            {
                var cancelAction = UIAlertAction.Create(cancelButtonText, UIAlertActionStyle.Cancel, (act) => { tcs.SetResult(false); });
                confirmDialog.AddAction(cancelAction);
            }

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(confirmDialog, true, null);

            return await tcs.Task;
        }
    }
}
