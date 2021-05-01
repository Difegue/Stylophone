using FluentMPC.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Stylophone.Common.Helpers;
using Stylophone.Common.Services;
using Stylophone.Common.Interfaces;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace FluentMPC.Views
{
    public sealed partial class NowPlayingBar
    {
        public PlaybackViewModel PlaybackViewModel => (PlaybackViewModel)DataContext;

        private MPDConnectionService _mpdService;
        private IDispatcherService _dispatcherService;

        public NowPlayingBar()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<PlaybackViewModel>();
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //TODO hacky
            _mpdService = Ioc.Default.GetRequiredService<MPDConnectionService>();
            _dispatcherService = Ioc.Default.GetRequiredService<IDispatcherService>();

            _mpdService.StatusChanged += PlaybackSession_PlaybackStateChanged;
            _mpdService.ConnectionChanged += MPDConnectionService_ConnectionLost;

            UpdateBars();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _mpdService.StatusChanged -= PlaybackSession_PlaybackStateChanged;
            _mpdService.ConnectionChanged -= MPDConnectionService_ConnectionLost;
        }

        /// <summary>
        /// Show a teachingtip when connection to the server is lost.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MPDConnectionService_ConnectionLost(object sender, EventArgs e) => UpdateBars();

        private void UpdateBars()
        {
            _dispatcherService.ExecuteOnUIThreadAsync(() =>
            {
                if (!_mpdService.IsConnected)
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
            Task.Run(() => PlaybackSession_PlaybackStateChangedAsync(sender, eventArgs)).ConfigureAwait(false);
        }

        private async Task PlaybackSession_PlaybackStateChangedAsync(object sender, EventArgs eventArgs)
        {
            await _dispatcherService.ExecuteOnUIThreadAsync(() =>
            {
                switch (_mpdService.CurrentStatus.State)
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
