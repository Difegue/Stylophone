using System;
using System.Threading.Tasks;

using Stylophone.Views;

using Microsoft.Toolkit.Uwp.Helpers;

using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Stylophone.Services
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
            var result = ContentDialogResult.None;

            await DispatcherHelper.ExecuteOnUIThreadAsync (async () => result = await dialog.ShowAsync());

            // Return new playlist name if checked, selected playlist otherwise
            return result == ContentDialogResult.Primary ? dialog.AddNewPlaylist ? dialog.PlaylistName : dialog.SelectedPlaylist : null;
        }

        private static bool shown = false;
        internal static async Task ShowFirstRunDialogIfAppropriateAsync()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, async () =>
                {
                    if (SystemInformation.IsFirstRun && !shown)
                    {
                        shown = true;
                        var dialog = new FirstRunDialog();
                        await dialog.ShowAsync();
                    }
                });
        }
    }
}
