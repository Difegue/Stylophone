using System;
using FluentMPC.Helpers;
using FluentMPC.Services;
using FluentMPC.ViewModels;
using FluentMPC.ViewModels.Items;
using Microsoft.Toolkit.Uwp.UI.Animations;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentMPC.Views
{
    public sealed partial class SearchResultsPage : Page
    {
        public SearchResultsViewModel ViewModel { get; } = new SearchResultsViewModel();

        public SearchResultsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string searchText)
            {
                ViewModel.InitializeAsync(searchText);
            }
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
