using System;

using FluentMPC.ViewModels;

using Windows.UI.Xaml;
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
    }
}
