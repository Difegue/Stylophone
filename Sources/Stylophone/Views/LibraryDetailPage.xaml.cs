using Stylophone.Helpers;
using Microsoft.Toolkit.Uwp.UI.Animations;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Stylophone.Common.ViewModels;
using MvvmCross;
using Stylophone.Common.Interfaces;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using MvvmCross.Platforms.Uap.Views;

namespace Stylophone.Views
{
    [MvxViewFor(typeof(AlbumDetailViewModel))]
    public sealed partial class LibraryDetailPage : MvxWindowsPage
    {
        public AlbumDetailViewModel Vm => (AlbumDetailViewModel)ViewModel;

        private IMvxNavigationService _navigationService;

        public LibraryDetailPage()
        {
            InitializeComponent();

            // TODO hacky
            _navigationService = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ((Page)this).RegisterElementForConnectedAnimation("animationKeyLibrary", itemHero);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                //_navigationService.SetListDataItemForNextConnectedAnimation(ViewModel.Item);
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
