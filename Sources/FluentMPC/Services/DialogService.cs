using System;
using System.Threading.Tasks;

using FluentMPC.Views;

using Microsoft.Toolkit.Uwp.Helpers;

using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace FluentMPC.Services
{
    public static class DialogService
    {

        public static async Task<string> ShowAddToPlaylistDialog()
        {
            var dialog = new AddToPlaylistDialog();
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
