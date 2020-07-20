using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using FluentMPC.Core.Models;
using FluentMPC.Core.Services;
using FluentMPC.Helpers;
using FluentMPC.ViewModels.Items;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET.Types;
using Windows.Media.Audio;

namespace FluentMPC.ViewModels
{
    public class LibraryDetailViewModel : Observable
    {
        private AlbumViewModel _item;

        public AlbumViewModel Item
        {
            get { return _item; }
            set { Set(ref _item, value); }
        }

        public ObservableCollection<TrackViewModel> Source { get; } = new ObservableCollection<TrackViewModel>();

        public LibraryDetailViewModel()
        {
        }

        public async Task InitializeAsync(AlbumViewModel album)
        {
            Item = album;

            if (album.IsFullyLoaded)
            {
                // Already loaded, create tracks now
                CreateTrackViewModels();
            }
            else if (album.IsDetailLoading)
            {
                // AlbumVM is currently loading, wait for it to conclude to create tracks
                album.PropertyChanged += async (s, e) =>
                {
                    if (album.IsFullyLoaded && Source.Count == 0)
                        await DispatcherHelper.ExecuteOnUIThreadAsync(() => CreateTrackViewModels());
                };
            }
            else // AlbumVM hasn't been loaded at all, load it ourselves
            {
                _ = Task.Run(async () =>
                {
                    await album.LoadAlbumDataAsync();
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() => CreateTrackViewModels());
                });
            }

        }

        private void CreateTrackViewModels()
        {
            foreach (IMpdFile file in Item.Files)
            {
                Source.Add(new TrackViewModel(file));
            }
        }
    }
}
