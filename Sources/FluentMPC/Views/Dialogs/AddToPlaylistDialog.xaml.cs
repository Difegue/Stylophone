using FluentMPC.Services;
using MpcNET.Types;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentMPC.Views
{
    public sealed partial class AddToPlaylistDialog : ContentDialog
    {
        public AddToPlaylistDialog()
        {
            RequestedTheme = (Window.Current.Content as FrameworkElement).RequestedTheme;
            Playlists = new ObservableCollection<MpdPlaylist>(MPDConnectionService.Playlists);
            InitializeComponent();
        }

        public ObservableCollection<MpdPlaylist> Playlists { get; internal set; }
        public string SelectedPlaylist { get; internal set; }

        private void Update_Selected(object sender, SelectionChangedEventArgs e)
        {
            SelectedPlaylist = (sender as ComboBox).SelectedValue as string;
        }
    }
}
