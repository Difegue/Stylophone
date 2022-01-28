using Stylophone.Helpers;
using Microsoft.Toolkit.Uwp.UI.Animations;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Stylophone.Common.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Stylophone.Common.Interfaces;

namespace Stylophone.Views
{
    public sealed partial class LibraryDetailPage : Page
    {
        public AlbumDetailViewModel ViewModel => (AlbumDetailViewModel)DataContext;

        private INavigationService _navigationService;

        public LibraryDetailPage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<AlbumDetailViewModel>();

            // TODO hacky
            _navigationService = Ioc.Default.GetRequiredService<INavigationService>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.RegisterElementForConnectedAnimation("animationKeyLibrary", itemHero);
            if (e.Parameter is AlbumViewModel album)
            {
               ViewModel.Initialize(album);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                _navigationService.SetListDataItemForNextConnectedAnimation(ViewModel.Item);
            }
        }

        private void Queue_Track(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            var listView = sender as ListView;
            var trackVm = listView.SelectedItem as TrackViewModel;
            trackVm.AddToQueueCommand.Execute(trackVm.File);
        }

        private void Select_Item(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e) => UWPHelpers.SelectItemOnFlyoutRightClick<TrackViewModel>(QueueList, e);
    }
}
