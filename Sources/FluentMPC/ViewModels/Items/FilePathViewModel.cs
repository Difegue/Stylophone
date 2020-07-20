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

        private ObservableCollection<FilePathViewModel> _childPaths;
        public ObservableCollection<FilePathViewModel> Children {
            get => _childPaths;
            set => Set(ref _childPaths, value);
        }

        public async Task UpdateChildrenAsync()
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
            });
        }

        // TODO - annoying to implement it seems
        public ICommand PlayTrackNextCommand;

        private ICommand _playCommand;
        public ICommand PlayCommand => _playCommand ?? (_playCommand = new RelayCommand(PlayPath));

        private async void PlayPath()
        {
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                //TODO - clear, add then play
                //var response = await c.InternalResource.SendAsync(new PlayCommand(file.Id));
            }
        }

        private ICommand _addToQueueCommand;
        public ICommand AddToQueueCommand => _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand(AddToQueue));

        private async void AddToQueue()
        {
            // AddCommand adds either the full directory or the song, depending on the path given.
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var response = await c.InternalResource.SendAsync(new AddCommand(Path));
            }
        }

        public ICommand AddToPlayListCommand;
        // TODO add to playlist command


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
                //TODO - resource
                _childPaths.Add(new FilePathViewModel("Loading...")); 
            }
            
        }

        public FilePathViewModel(string name)
        {
            Name = name;
        }

    }
}
