using System;

using Stylophone.ViewModels;
using Stylophone.ViewModels.Items;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Stylophone.Views
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

        // Propagate DataContext of the ListViewItem to the MenuFlyout.
        // https://github.com/microsoft/microsoft-ui-xaml/issues/911
        private void MenuFlyout_Opening(object sender, object e)
        {
            var menuFlyout = (MenuFlyout)sender;
            var dataContext = menuFlyout.Target?.DataContext ?? (menuFlyout.Target as ContentControl)?.Content;
            if (dataContext != null)
            {
                foreach (var item in menuFlyout.Items)
                {
                    var menuFlyoutItem = item as MenuFlyoutItem;

                    if (menuFlyoutItem != null)
                    {
                        menuFlyoutItem.DataContext = dataContext;
                    }
                }
            }
            else
            {
                menuFlyout.Hide();
            }

        }

        private void Queue_Track(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            var listView = sender as Helpers.AlternatingRowListView;
            var trackVm = listView.SelectedItem as TrackViewModel;
            trackVm.AddToQueueCommand.Execute(trackVm.File);
        }
    }
}
