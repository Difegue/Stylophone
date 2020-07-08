using System;

using FluentMPC.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentMPC.Views
{
    public sealed partial class PlaylistsPage : Page
    {
        public PlaylistsViewModel ViewModel { get; } = new PlaylistsViewModel();

        public PlaylistsPage()
        {
            InitializeComponent();
            Loaded += PlaylistsPage_Loaded;
        }

        private async void PlaylistsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadDataAsync(MasterDetailsViewControl.ViewState);
        }
    }
}
