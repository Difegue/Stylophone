using FluentMPC.Services;
using FluentMPC.ViewModels.Playback;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Navigation;

namespace FluentMPC.Views
{
    /// <summary>
    /// Compact Overlay view. Mostly borrowed from SoundByte.
    /// </summary>
    public sealed partial class OverlayView
    {
        public PlaybackViewModel PlaybackViewModel { get; private set; }
        private int _mainAppViewId;

        public OverlayView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PlaybackViewModel = new PlaybackViewModel(CoreWindow.GetForCurrentThread().Dispatcher, 500);
            _mainAppViewId = (int)e.Parameter;

            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

        }

        private async void NavigateToMainView()
        {
            var currentViewId = -1;

            await DispatcherHelper.AwaitableRunAsync(CoreWindow.GetForCurrentThread().Dispatcher,() =>
            { 
                currentViewId = ApplicationView.GetForCurrentView().Id;
            });

            await ApplicationViewSwitcher.TryShowAsViewModeAsync(_mainAppViewId, ApplicationViewMode.Default);

            // Switch to this window
            await ApplicationViewSwitcher.SwitchAsync(_mainAppViewId, currentViewId,
                ApplicationViewSwitchingOptions.ConsolidateViews);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            PlaybackViewModel?.Dispose();
            PlaybackViewModel = null;
        }

        private void Page_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PlaybackViewModel?.Dispose();
            PlaybackViewModel = null;
        }
    }
}
