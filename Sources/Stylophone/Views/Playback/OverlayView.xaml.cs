using Stylophone.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit;
using Stylophone.Common.Helpers;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Navigation;

namespace Stylophone.Views
{
    /// <summary>
    /// Compact Overlay view. Simplified variant using only AppWindow.
    /// </summary>
    public sealed partial class OverlayView
    {
        public PlaybackViewModel PlaybackViewModel => (PlaybackViewModel)DataContext;

        public AppWindow HostAppWindow { get; set; }

        public OverlayView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = Ioc.Default.GetRequiredService<PlaybackViewModel>();
            PlaybackViewModel.HostType = VisualizationType.OverlayPlayback;
        }

        private async Task NavigateToMainView()
        {
            await HostAppWindow.CloseAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            PlaybackViewModel?.Dispose();
            DataContext = null;
        }

        private void Page_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PlaybackViewModel?.Dispose();
            DataContext = null;
        }
    }
}
