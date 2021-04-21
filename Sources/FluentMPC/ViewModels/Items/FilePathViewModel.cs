using FluentMPC.Helpers;
using FluentMPC.Services;
using MpcNET.Commands.Database;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Types;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using MpcNET.Commands.Queue;
using MpcNET.Commands.Reflection;
using System.Collections.ObjectModel;
using MpcNET;

namespace FluentMPC.ViewModels.Items
{
    public class FilePathViewModel : Observable
    {
        private string _path;

        public string Path
        {
            get => _path;
            private set => Set(ref _path, value);
        }

        private string _name;

        public string Name
        {
            get => _name;
            private set => Set(ref _name, value);
        }

        public bool IsDirectory { get; set; }
        public bool IsLoaded { get; set; }

        private RangedObservableCollection<FilePathViewModel> _childPaths;
        public RangedObservableCollection<FilePathViewModel> Children
        {
            get => _childPaths;
            set => Set(ref _childPaths, value);
        }

        private bool _isLoadingChildren;
        public async Task LoadChildrenAsync()
        {
            if (IsLoaded || _isLoadingChildren || IsDirectory == false || _childPaths == null || Path == null) return;

            _isLoadingChildren = true;
            try
            {
                var newChildren = new List<FilePathViewModel>();

                var response = await MPDConnectionService.SafelySendCommandAsync(new LsInfoCommand(Path));

                if (response != null)
                    foreach (var item in response)
                    {
                        newChildren.Add(new FilePathViewModel(item));
                    }
                else
                    newChildren.Add(new FilePathViewModel("💥 Failed"));

                await DispatcherService.ExecuteOnUIThreadAsync(() =>
                {
                    _childPaths.AddRange(newChildren);
                    _childPaths.RemoveAt(0); // Remove the placeholder after adding the new items, otherwise the treeitem can close back up
                    IsLoaded = true;
                });
            }
            finally
            {
                _isLoadingChildren = false;
            }
        }

        private ICommand _playCommand;
        public ICommand PlayCommand => _playCommand ?? (_playCommand = new RelayCommand(PlayPath));

        private async void PlayPath()
        {
            // Clear queue, add path and play
            var commandList = new CommandList(new IMpcCommand<object>[] { new ClearCommand(), new AddCommand(Path), new PlayCommand(0) });

            if (await MPDConnectionService.SafelySendCommandAsync(commandList) != null)
            {
                NotificationService.ShowInAppNotification(string.Format("NowPlayingText".GetLocalized(), Path));
            }
        }

        private ICommand _addToQueueCommand;
        public ICommand AddToQueueCommand => _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand(AddToQueue));

        private async void AddToQueue()
        {
            // AddCommand adds either the full directory or the song, depending on the path given.
            var response = await MPDConnectionService.SafelySendCommandAsync(new AddCommand(Path));

            if (response != null)
                NotificationService.ShowInAppNotification("AddedToQueueText".GetLocalized());
        }

        private ICommand _addToPlaylistCommand;
        public ICommand AddToPlaylistCommand => _addToPlaylistCommand ?? (_addToPlaylistCommand = new RelayCommand(AddToPlaylist));
        private async void AddToPlaylist()
        {
            var playlistName = await DialogService.ShowAddToPlaylistDialog();
            if (playlistName == null) return;

            var response = await MPDConnectionService.SafelySendCommandAsync(new PlaylistAddCommand(playlistName, Path));

            if (response != null)
                NotificationService.ShowInAppNotification(string.Format("AddedToPlaylistText".GetLocalized(), playlistName));
        }

        public FilePathViewModel(IMpdFilePath file)
        {
            Path = file.Path;
            Name = file.Name ?? Path.Split('/').Last();

            // If it's a directory, add children
            if (file is MpdDirectory)
            {
                IsDirectory = true;
                _childPaths = new RangedObservableCollection<FilePathViewModel>();

                // Add a bogus child that'll be replaced when the list is loaded
                _childPaths.Add(new FilePathViewModel("LoadingTreeItem".GetLocalized()));
            }

        }

        public FilePathViewModel(string name)
        {
            Name = name;
            _childPaths = new RangedObservableCollection<FilePathViewModel>();
        }

    }
}
