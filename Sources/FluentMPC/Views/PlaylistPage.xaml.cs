using System;
using FluentMPC.Helpers;
using FluentMPC.ViewModels;
using FluentMPC.ViewModels.Items;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentMPC.Views
{
    public sealed partial class PlaylistPage : Page
    {
        public PlaylistViewModel ViewModel { get; } = new PlaylistViewModel();

        public PlaylistPage()
        {
            InitializeComponent();
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

        private void Select_Item(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e) => MiscHelpers.SelectItemOnFlyoutRightClick<TrackViewModel>(QueueList, e);
    }
}
