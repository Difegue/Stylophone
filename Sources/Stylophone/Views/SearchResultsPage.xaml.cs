using Stylophone.Helpers;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Stylophone.Common.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Stylophone.Views
{
    public sealed partial class SearchResultsPage : Page
    {
        public SearchResultsViewModel ViewModel => (SearchResultsViewModel)DataContext;

        public SearchResultsPage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<SearchResultsViewModel>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string searchText)
            {
                ViewModel.Initialize(searchText);
            }
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
