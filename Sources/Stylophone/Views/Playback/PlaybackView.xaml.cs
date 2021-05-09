using Stylophone.ViewModels;
using Stylophone.Common.Helpers;
using Windows.UI.Xaml.Navigation;
using MvvmCross.Platforms.Uap.Views;
using MvvmCross.ViewModels;

namespace Stylophone.Views
{
    [MvxViewFor(typeof(PlaybackViewModel))]
    public sealed partial class PlaybackView : MvxWindowsPage
    {
        public PlaybackViewModel Vm => (PlaybackViewModel)ViewModel;

        public PlaybackView()
        {
            InitializeComponent();
        }

        protected override void OnViewModelSet()
        {
            if (Vm != null)
                Vm.HostType = VisualizationType.FullScreenPlayback;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (Vm != null)
                Vm.HostType = VisualizationType.NowPlayingBar;
            ViewModel = null;
        }

        private void Page_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (Vm != null)
                Vm.HostType = VisualizationType.NowPlayingBar;
            ViewModel = null;
        }
    }
}
