using System;
using System.Threading.Tasks;

using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.iOS.ViewControllers;
using Strings = Stylophone.Localization.Strings.Resources;
using UIKit;
using StoreKit;
using Stylophone.Common.ViewModels;

namespace Stylophone.iOS.Services
{
    public class DialogService : IDialogService
    {
        private IDispatcherService _dispatcherService;
        private IApplicationStorageService _storageService;
        private IInteropService _interop;
        private INavigationService _navigationService;
        private MPDConnectionService _mpdService;

        public DialogService(IDispatcherService dispatcherService, IApplicationStorageService storageService, IInteropService interop, INavigationService navigationService, MPDConnectionService mpdService)
        {
            _dispatcherService = dispatcherService;
            _storageService = storageService;
            _navigationService = navigationService;
            _mpdService = mpdService;
            _interop = interop;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowExistingPlaylists">If set to FALSE, the dialog will only allow you to create a new playlist.</param>
        /// <returns></returns>
        public async Task<string> ShowAddToPlaylistDialog(bool allowExistingPlaylists = true)
        {
            // Wrap VC in a NavigationController for top controls
            var dialog = new AddToPlaylistViewController(_mpdService, allowExistingPlaylists);
            var navigationController = new UINavigationController(dialog);

            // Specify medium detent for the NavigationController's presentation
            UISheetPresentationController uspc = (UISheetPresentationController)navigationController.PresentationController;
            uspc.Detents = new [] { UISheetPresentationControllerDetent.CreateMediumDetent() };
            
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(navigationController, true, null);

            var result = await dialog.CompletionTask;

            // Return new playlist name if checked, selected playlist otherwise
            return result ? dialog.AddNewPlaylist ? dialog.PlaylistName : dialog.SelectedPlaylist : null;
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

                        await ShowConfirmDialogAsync(Strings.FirstRunTitle, Strings.FirstRunText, Strings.OKButtonText);
                        _navigationService.Navigate<SettingsViewModel>();
                    }
                });
        }

        public Task ShowRateAppDialogIfAppropriateAsync()
        {
            if (_storageService.GetValue<int>("LaunchCount") >= 4 && !_storageService.GetValue<bool>("HasSeenRateAppPrompt"))
            {
                SKStoreReviewController.RequestReview();
                _storageService.SetValue("HasSeenRateAppPrompt", true);
            }
            return Task.CompletedTask;
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
