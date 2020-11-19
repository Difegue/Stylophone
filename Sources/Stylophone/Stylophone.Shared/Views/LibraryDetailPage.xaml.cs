using System;

using Stylophone.Services;
using Stylophone.ViewModels;
using Stylophone.ViewModels.Items;
using Microsoft.Toolkit.Uwp.UI.Animations;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Stylophone.Views
{
    public sealed partial class LibraryDetailPage : Page
    {
        public AlbumDetailViewModel ViewModel { get; } = new AlbumDetailViewModel();

        public LibraryDetailPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.RegisterElementForConnectedAnimation("animationKeyLibrary", itemHero);
            if (e.Parameter is AlbumViewModel album)
            {
                await ViewModel.InitializeAsync(album);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                NavigationService.Frame.SetListDataItemForNextConnectedAnimation(ViewModel.Item);
            }
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
