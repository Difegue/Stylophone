using MvvmCross;
using Stylophone.ViewModels;

namespace Stylophone.Views
{
    public sealed partial class NowPlayingBar
    {
        public PlaybackViewModel PlaybackViewModel => (PlaybackViewModel)DataContext;

        public NowPlayingBar()
        {
            InitializeComponent();
            DataContext = Mvx.IoCProvider.IoCConstruct<PlaybackViewModel>();
        }

        private void Volume_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
            PlaybackViewModel.MediaVolume += 2 * delta / 120;
        }

    }
}
