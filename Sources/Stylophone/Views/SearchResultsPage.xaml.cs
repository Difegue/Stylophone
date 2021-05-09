using Stylophone.Helpers;
using Stylophone.Common.ViewModels;
using MvvmCross.ViewModels;
using MvvmCross.Platforms.Uap.Views;

namespace Stylophone.Views
{
    [MvxViewFor(typeof(SearchResultsViewModel))]
    public sealed partial class SearchResultsPage : MvxWindowsPage
    {
        public SearchResultsViewModel Vm => (SearchResultsViewModel)ViewModel;

        public SearchResultsPage()
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
