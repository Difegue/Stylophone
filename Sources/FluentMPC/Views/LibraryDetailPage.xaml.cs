using System;
using FluentMPC.Helpers;
using FluentMPC.Services;
using FluentMPC.ViewModels;
using FluentMPC.ViewModels.Items;
using Microsoft.Toolkit.Uwp.UI.Animations;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace FluentMPC.Views
{
    public sealed partial class LibraryDetailPage : Page
    {
        public AlbumDetailViewModel ViewModel { get; } = new AlbumDetailViewModel();

        public LibraryDetailPage()
        {
            InitializeComponent();

            var headerShadow = new ThemeShadow();
            headerShadow.Receivers.Add(QueueList);
            
            QueueList.Header.Shadow = headerShadow;
            QueueList.Header.Translation += new Vector3(0, 0, 32);
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
                NavigationService.Frame.SetListDataItemForNextConnectedAnimation(ViewModel.Item);
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
