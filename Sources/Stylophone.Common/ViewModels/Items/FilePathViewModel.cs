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
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Stylophone.Common.ViewModels
{
    public class FilePathViewModelFactory
    {
        public INotificationService NotificationService;
        public IDispatcherService DispatcherService;
        public INavigationService NavigationService;
        public IDialogService DialogService;
        public AlbumArtService AlbumArtService;
        public MPDConnectionService MPDService;

        public FilePathViewModelFactory(INotificationService notificationService, IDispatcherService dispatcherService, INavigationService navigationService, IDialogService dialogService, AlbumArtService albumArtService, MPDConnectionService mpdService)
        {
            DispatcherService = dispatcherService;
            NotificationService = notificationService;
            NavigationService = navigationService;
            DialogService = dialogService;
            AlbumArtService = albumArtService;
            MPDService = mpdService;
        }

        public FilePathViewModel GetFilePathViewModel(IMpdFilePath file, FilePathViewModel parent)
        {
            return new FilePathViewModel(this, file, parent);
        }
    }

    public partial class FilePathViewModel : ViewModelBase
    {
        private INotificationService _notificationService;
        private IDialogService _dialogService;
        private MPDConnectionService _mpdService;
        private FilePathViewModelFactory _filePathFactory;

        internal FilePathViewModel(FilePathViewModelFactory factory, IMpdFilePath file, FilePathViewModel parent): base(factory.DispatcherService)
        {
            _filePathFactory = factory;
            _notificationService = factory.NotificationService;
            _dialogService = factory.DialogService;
            _mpdService = factory.MPDService;


            Path = file.Path;
            Parent = parent;
            Name = file.Name ?? Path.Split('/').Last();

            // If it's a directory, add children
            if (file is MpdDirectory)
            {
                IsDirectory = true;
                _children = new RangedObservableCollection<FilePathViewModel>();

                // Add a bogus child that'll be replaced when the list is loaded
                _children.Add(new FilePathViewModel(Resources.FoldersLoadingTreeItem, this, _dispatcherService));
            }

        }

        public FilePathViewModel(string name, FilePathViewModel parent, IDispatcherService dispatcherService): base(dispatcherService)
        {
            Name = name;
            Parent = parent;
            _children = new RangedObservableCollection<FilePathViewModel>();
        }

        [ObservableProperty]
        private string _path;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private bool _isLoaded;

        [ObservableProperty]
        private RangedObservableCollection<FilePathViewModel> _children;

        public bool IsDirectory { get; set; }
        public FilePathViewModel Parent { get; }

        private bool _isLoadingChildren;
        public async Task LoadChildrenAsync()
        {
            if (IsLoaded || _isLoadingChildren || IsDirectory == false || _children == null || Path == null) return;

            _isLoadingChildren = true;
            try
            {
                var newChildren = new List<FilePathViewModel>();

                var response = await _mpdService.SafelySendCommandAsync(new LsInfoCommand(Path));

                if (response != null)
                    foreach (var item in response)
                    {
                        newChildren.Add(_filePathFactory.GetFilePathViewModel(item, this));
                    }
                else
                    newChildren.Add(new FilePathViewModel("💥 Failed", this, _dispatcherService));

                await _dispatcherService.ExecuteOnUIThreadAsync(() =>
                {
                    _children.AddRange(newChildren);
                    _children.RemoveAt(0); // Remove the placeholder after adding the new items, otherwise the treeitem can close back up
                    IsLoaded = true;
                });
            }
            finally
            {
                _isLoadingChildren = false;
            }
        }


        [RelayCommand]
        private async void Play()
        {
            // Clear queue, add path and play
            var commandList = new CommandList(new IMpcCommand<object>[] { new ClearCommand(), new AddCommand(Path), new PlayCommand(0) });

            if (await _mpdService.SafelySendCommandAsync(commandList) != null)
            {
                _notificationService.ShowInAppNotification(string.Format(Resources.NotificationNowPlayingTrack, Path));
            }
        }

        [RelayCommand]
        private async void AddToQueue()
        {
            // AddCommand adds either the full directory or the song, depending on the path given.
            var response = await _mpdService.SafelySendCommandAsync(new AddCommand(Path));

            if (response != null)
                _notificationService.ShowInAppNotification(Resources.NotificationAddedToQueue);
        }

        [RelayCommand]
        private async void AddToPlaylist()
        {
            var playlistName = await _dialogService.ShowAddToPlaylistDialog();
            if (playlistName == null) return;

            var response = await _mpdService.SafelySendCommandAsync(new PlaylistAddCommand(playlistName, Path));

            if (response != null)
                _notificationService.ShowInAppNotification(string.Format(Resources.NotificationAddedToPlaylist, playlistName));
        }

    }
}
