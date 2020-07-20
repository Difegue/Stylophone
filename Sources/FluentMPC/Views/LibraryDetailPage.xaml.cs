using System;

using FluentMPC.Services;
using FluentMPC.ViewModels;
using FluentMPC.ViewModels.Items;
using Microsoft.Toolkit.Uwp.UI.Animations;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentMPC.Views
{
    public sealed partial class LibraryDetailPage : Page
    {
        public LibraryDetailViewModel ViewModel { get; } = new LibraryDetailViewModel();

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
    }
}
