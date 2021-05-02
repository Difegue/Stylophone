using Stylophone.Services;
using MpcNET.Types;
using Stylophone.Common.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stylophone.Views
{
    public sealed partial class AddToPlaylistDialog : ContentDialog, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public AddToPlaylistDialog(MPDConnectionService mpdService, bool allowExistingPlaylists)
        {
            AllowExistingPlaylists = allowExistingPlaylists;

            if (!allowExistingPlaylists)
            {
                AddNewPlaylist = true;
            }
            RequestedTheme = (Window.Current.Content as FrameworkElement).RequestedTheme;
            Playlists = new ObservableCollection<MpdPlaylist>(mpdService.Playlists);
            InitializeComponent();
        }

        public ObservableCollection<MpdPlaylist> Playlists { get; internal set; }
        public string SelectedPlaylist { get; internal set; }
        public string PlaylistName { get; internal set; }

        public bool AddNewPlaylist { get; internal set; }
        public bool AllowExistingPlaylists { get; internal set; }

        private void Update_Selected(object sender, SelectionChangedEventArgs e)
        {
            SelectedPlaylist = (sender as ComboBox).SelectedValue as string;
        }

        private void Update_Checkbox(object sender, RoutedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AddNewPlaylist)));
        }
    }
}
