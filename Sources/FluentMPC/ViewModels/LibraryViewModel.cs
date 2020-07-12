using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

using FluentMPC.Core.Models;
using FluentMPC.Core.Services;
using FluentMPC.Helpers;
using FluentMPC.Services;
using FluentMPC.ViewModels.Items;
using FluentMPC.Views;

using Microsoft.Toolkit.Uwp.UI.Animations;
using MpcNET.Commands.Database;
using MpcNET.Tags;

namespace FluentMPC.ViewModels
{
    public class LibraryViewModel : Observable
    {
        private ICommand _itemClickCommand;

        public ICommand ItemClickCommand => _itemClickCommand ?? (_itemClickCommand = new RelayCommand<AlbumViewModel>(OnItemClick));

        public ObservableCollection<AlbumViewModel> Source { get; } = new ObservableCollection<AlbumViewModel>();

        public LibraryViewModel()
        {
        }

        public async Task LoadDataAsync()
        {
            Source.Clear();

            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var response = await c.SendAsync(new ListCommand(MpdTags.Album));

                if (response.IsResponseValid)
                    foreach (var item in response.Response.Content)
                    {
                        Source.Add(new AlbumViewModel(item));
                    }
            }
        }

        private void OnItemClick(AlbumViewModel clickedItem)
        {
            if (clickedItem != null)
            {
                NavigationService.Frame.SetListDataItemForNextConnectedAnimation(clickedItem);
                NavigationService.Navigate<LibraryDetailPage>(clickedItem.Name);
            }
        }
    }
}
