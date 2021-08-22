using Stylophone.ViewModels;
using Stylophone.Common.Helpers;
using Windows.UI.Xaml.Navigation;

namespace Stylophone.Views
{
    public sealed partial class PlaybackView
    {
        public PlaybackViewModel PlaybackViewModel { get; private set; }

        public PlaybackView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PlaybackViewModel = (PlaybackViewModel)e.Parameter;
            PlaybackViewModel.HostType = VisualizationType.FullScreenPlayback;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (PlaybackViewModel != null)
                PlaybackViewModel.HostType = VisualizationType.NowPlayingBar;
            PlaybackViewModel = null;
        }

        private void Page_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (PlaybackViewModel != null)
                PlaybackViewModel.HostType = VisualizationType.NowPlayingBar;
            PlaybackViewModel = null;
        }
    }
}
