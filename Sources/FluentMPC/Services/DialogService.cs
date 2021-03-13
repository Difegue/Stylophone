using System;
using System.Threading.Tasks;

using FluentMPC.Views;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;

using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace FluentMPC.Services
{
    public static class DialogService
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowExistingPlaylists">If set to FALSE, the dialog will only allow you to create a new playlist.</param>
        /// <returns></returns>
        public static async Task<string> ShowAddToPlaylistDialog(bool allowExistingPlaylists = true)
        {
            var dialog = new AddToPlaylistDialog(allowExistingPlaylists);
            var result = await DispatcherService.DispatcherQueue.EnqueueAsync(async () => await dialog.ShowAsync());

            // Return new playlist name if checked, selected playlist otherwise
            return result == ContentDialogResult.Primary ? dialog.AddNewPlaylist ? dialog.PlaylistName : dialog.SelectedPlaylist : null;
        }

        private static bool shown = false;
        internal static async Task ShowFirstRunDialogIfAppropriateAsync()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, async () =>
                {
                    if (SystemInformation.Instance.IsFirstRun && !shown)
                    {
                        shown = true;
                        var dialog = new FirstRunDialog();
                        await dialog.ShowAsync();
                    }
                });
        }
    }
}
