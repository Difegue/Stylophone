using Stylophone.Helpers;
using Stylophone.Common.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MvvmCross;
using MvvmCross.ViewModels;
using MvvmCross.Platforms.Uap.Views;

namespace Stylophone.Views
{
    [MvxViewFor(typeof(PlaylistViewModel))]
    public sealed partial class PlaylistPage : MvxWindowsPage
    {
        public PlaylistViewModel Vm => (PlaylistViewModel)ViewModel;

        public PlaylistPage()
        {
            InitializeComponent();
        }

        private void Queue_Track(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            var listView = sender as AlternatingRowListView;
            var trackVm = listView.SelectedItem as TrackViewModel;
            trackVm.AddToQueueCommand.Execute(trackVm.File);
        }

        private void Select_Item(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e) => UWPHelpers.SelectItemOnFlyoutRightClick<TrackViewModel>(QueueList, e);
    }
}
