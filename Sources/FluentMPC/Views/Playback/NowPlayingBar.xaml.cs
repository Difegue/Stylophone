using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Controls;
using FluentMPC.ViewModels.Playback;
using System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using FluentMPC.Services;

namespace FluentMPC.Views
{
    public sealed partial class NowPlayingBar
    {
        public PlaybackViewModel PlaybackViewModel { get; } = new PlaybackViewModel(CoreWindow.GetForCurrentThread().Dispatcher, VisualizationType.NowPlayingBar);

        public NowPlayingBar() => InitializeComponent();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            MPDConnectionService.StatusChanged += PlaybackSession_PlaybackStateChanged;
            MPDConnectionService.ConnectionChanged += MPDConnectionService_ConnectionLost;

            UpdateBars();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            MPDConnectionService.StatusChanged -= PlaybackSession_PlaybackStateChanged;
            MPDConnectionService.ConnectionChanged -= MPDConnectionService_ConnectionLost;
        }

        /// <summary>
        /// Show a teachingtip when connection to the server is lost.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MPDConnectionService_ConnectionLost(object sender, EventArgs e) => UpdateBars();

        private void UpdateBars()
        {
            DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                if (!MPDConnectionService.IsConnected)
                {
                    LoadingBar.Visibility = Visibility.Visible;
                    ProgressBar.Visibility = Visibility.Collapsed;
                }
                else
                {
                    LoadingBar.Visibility = Visibility.Collapsed;
                    ProgressBar.Visibility = Visibility.Visible;
                }
            });
        }

        /// <summary>
        ///     Show the loading UI when a track is loading
        /// </summary>
        private void PlaybackSession_PlaybackStateChanged(object sender, EventArgs eventArgs)
        {
            Task.Run(() => PlaybackSession_PlaybackStateChangedAsync(sender, eventArgs));
        }

        private async Task PlaybackSession_PlaybackStateChangedAsync(object sender, EventArgs eventArgs)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                switch (MPDConnectionService.CurrentStatus.State)
                {
                    case MpcNET.MpdState.Stop:
                    case MpcNET.MpdState.Play:
                    case MpcNET.MpdState.Pause:
                        LoadingBar.Visibility = Visibility.Collapsed;
                        ProgressBar.Visibility = Visibility.Visible;
                        break;

                    case MpcNET.MpdState.Unknown:
                        LoadingBar.Visibility = Visibility.Visible;
                        ProgressBar.Visibility = Visibility.Collapsed;
                        break;
                }
            });
        }

        private void Volume_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
            PlaybackViewModel.MediaVolume += 5 * delta / 120;
        }

    }
}
