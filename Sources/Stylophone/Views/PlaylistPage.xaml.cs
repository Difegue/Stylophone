using Stylophone.Helpers;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Stylophone.Common.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Stylophone.Views
{
    public sealed partial class PlaylistPage : Page
    {
        public PlaylistViewModel ViewModel => (PlaylistViewModel)DataContext;

        public PlaylistPage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<PlaylistViewModel>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadDataAsync(e.Parameter as string);
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
