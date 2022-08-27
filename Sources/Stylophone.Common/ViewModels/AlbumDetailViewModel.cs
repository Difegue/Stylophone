using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using MpcNET.Commands.Playlist;
using MpcNET.Commands.Queue;
using MpcNET.Commands.Reflection;
using MpcNET.Types;
using Stylophone.Common.Helpers;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.ViewModels
{
    public partial class AlbumDetailViewModel : ViewModelBase
    {

        private IDialogService _dialogService;
        private INotificationService _notificationService;
        private MPDConnectionService _mpdService;
        private TrackViewModelFactory _trackVmFactory;

        public AlbumDetailViewModel(IDialogService dialogService, INotificationService notificationService, IDispatcherService dispatcherService, MPDConnectionService mpdService, TrackViewModelFactory trackVmFactory):
            base(dispatcherService)
        {
            _dialogService = dialogService;
            _notificationService = notificationService;
            _mpdService = mpdService;
            _trackVmFactory = trackVmFactory;

            Source.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));
        }

        private AlbumViewModel _item;
        public AlbumViewModel Item
        {
            get { return _item; }
            set { Set(ref _item, value); }
        }

        private string _info;
        public string PlaylistInfo
        {
            get => _info;
            private set => Set(ref _info, value);
        }

        public ObservableCollection<TrackViewModel> Source { get; } = new ObservableCollection<TrackViewModel>();
        public bool IsSourceEmpty => Source.Count == 0;

        [RelayCommand]
        private async void AddToQueue(object list)
        {
            var selectedTracks = (IList<object>)list;

            if (selectedTracks?.Count > 0)
            {
                var commandList = new CommandList();

                foreach (var f in selectedTracks)
                {
                    var trackVM = f as TrackViewModel;
                    commandList.Add(new AddIdCommand(trackVM.File.Path));
                }

                var r = await _mpdService.SafelySendCommandAsync(commandList);
                if (r != null)
                    _notificationService.ShowInAppNotification(Resources.NotificationAddedToQueue);
            }
        }

        [RelayCommand]
        private async void AddToPlaylist(object list)
        {
            var playlistName = await _dialogService.ShowAddToPlaylistDialog();
            if (playlistName == null) return;

            var selectedTracks = (IList<object>)list;

            if (selectedTracks?.Count > 0)
            {
                var commandList = new CommandList();

                foreach (var f in selectedTracks)
                {
                    var trackVM = f as TrackViewModel;
                    commandList.Add(new PlaylistAddCommand(playlistName, trackVM.File.Path));
                }

                var req = await _mpdService.SafelySendCommandAsync(commandList);

                if (req != null)
                    _notificationService.ShowInAppNotification(string.Format(Resources.NotificationAddedToPlaylist, playlistName));
            }
        }

        public void Initialize(AlbumViewModel album)
        {
            Source.Clear();
            Item = album;

            if (album.AlbumArtLoaded)
            {
                // Already loaded, create tracks now
                CreateTrackViewModels();
            }
            else if (album.IsDetailLoading)
            {
                // AlbumVM is currently loading, wait for it to conclude to create tracks
                // We only look at IsDetailLoading since we don't need the album art to be loaded for tracks
                album.PropertyChanged += async (s, e) =>
                {
                    if (album.Files.Count > 0 && Source.Count == 0)
                        await _dispatcherService.ExecuteOnUIThreadAsync(() => CreateTrackViewModels());
                };
            }
            else // AlbumVM hasn't been loaded at all, load it ourselves
            {
                Task.Run(async () =>
                {
                    await album.LoadAlbumDataAsync();
                    await _dispatcherService.ExecuteOnUIThreadAsync(() => CreateTrackViewModels());
                }).ConfigureAwait(false);
            }

        }

        private void CreateTrackViewModels()
        {
            foreach (IMpdFile file in Item.Files)
            {
                Source.Add(_trackVmFactory.GetTrackViewModel(file));
            }

            var totalTime = Source.Select(vm => vm.File.Time).Aggregate((t1, t2) => t1 + t2);
            TimeSpan t = TimeSpan.FromSeconds(totalTime);

            PlaylistInfo = $"{Source.Count} Tracks, Total Time: {t.ToReadableString()}";
        }
    }
}
