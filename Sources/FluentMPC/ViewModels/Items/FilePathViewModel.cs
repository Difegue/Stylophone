using ColorThiefDotNet;
using FluentMPC.Helpers;
using FluentMPC.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using MpcNET.Commands.Database;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Color = Windows.UI.Color;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using Sundew.Base.Collections;
using System.Linq;

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

        private ObservableCollection<FilePathViewModel> _childPaths;
        public ObservableCollection<FilePathViewModel> Children
        {
            get => _childPaths;
            set => Set(ref _childPaths, value);
        }

        private bool _isLoadingChildren;
        public async Task LoadChildrenAsync()
        {
            if (_isLoadingChildren || IsDirectory == false || _childPaths == null || Path == null) return;

            _isLoadingChildren = true;
            try
            {
                var newChildren = new List<FilePathViewModel>();
                using (var c = await MPDConnectionService.GetConnectionAsync())
                {
                    var response = await c.InternalResource.SendAsync(new LsInfoCommand(Path));

                    if (response.IsResponseValid)
                        foreach (var item in response.Response.Content)
                        {
                            newChildren.Add(new FilePathViewModel(item));
                        }
                    else
                        newChildren.Add(new FilePathViewModel("💥 Failed"));
                }

                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    _childPaths.Clear();
                    _childPaths.AddRange(newChildren);
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
            // Clear queue, add album and play
            try
            {
                using (var c = await MPDConnectionService.GetConnectionAsync())
                {
                    var req = await c.InternalResource.SendAsync(new ClearCommand());
                    if (!req.IsResponseValid) throw new Exception($"Couldn't clear queue!");

                    req = await c.InternalResource.SendAsync(new AddCommand(Path));
                    req = await c.InternalResource.SendAsync(new PlayCommand(0));
                    NotificationService.ShowInAppNotification($"Now Playing {Path}");
                }
            }
            catch (Exception e)
            {
                NotificationService.ShowInAppNotification($"Couldn't play content: {e}", 0);
            }
        }

        private ICommand _addToQueueCommand;
        public ICommand AddToQueueCommand => _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand(AddToQueue));

        private async void AddToQueue()
        {
            // AddCommand adds either the full directory or the song, depending on the path given.
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var req = await c.InternalResource.SendAsync(new AddCommand(Path));

                if (!req.IsResponseValid)
                {
                    NotificationService.ShowInAppNotification($"Couldn't add file(s): Invalid MPD Response.", 0);
                    return;
                }
                NotificationService.ShowInAppNotification($"File(s) added to Queue!");
            }
        }

        private ICommand _addToPlaylistCommand;
        public ICommand AddToPlaylistCommand => _addToPlaylistCommand ?? (_addToPlaylistCommand = new RelayCommand(AddToPlaylist));
        private async void AddToPlaylist()
        {
            var playlistName = await DialogService.ShowAddToPlaylistDialog();
            if (playlistName == null) return;

            try
            {
                using (var c = await MPDConnectionService.GetConnectionAsync())
                {
                    var req = await c.InternalResource.SendAsync(new PlaylistAddCommand(playlistName, Path));

                    if (!req.IsResponseValid)
                    {
                        NotificationService.ShowInAppNotification($"Couldn't add file(s): Invalid MPD Response.", 0);
                        return;
                    }

                    NotificationService.ShowInAppNotification($"File(s) added to Playlist!");
                }
            }
            catch (Exception e)
            {
                NotificationService.ShowInAppNotification($"Couldn't add file(s): {e}", 0);
            }
        }


        public FilePathViewModel(IMpdFilePath file)
        {
            Path = file.Path;
            Name = file.Name ?? Path.Split('/').Last();

            // If it's a directory, add children
            if (file is MpdDirectory)
            {
                IsDirectory = true;
                _childPaths = new ObservableCollection<FilePathViewModel>();

                // Add a bogus child that'll be replaced when the list is loaded
                //TODO - put string in resource
                _childPaths.Add(new FilePathViewModel("Loading..."));
            }

        }

        public FilePathViewModel(string name)
        {
            Name = name;
        }

    }
}
