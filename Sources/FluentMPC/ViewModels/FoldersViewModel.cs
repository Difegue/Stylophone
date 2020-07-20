using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using FluentMPC.Core.Models;
using FluentMPC.Core.Services;
using FluentMPC.Helpers;
using FluentMPC.Services;
using FluentMPC.ViewModels.Items;
using MpcNET.Commands.Database;
using MpcNET.Types;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace FluentMPC.ViewModels
{
    public class FoldersViewModel : Observable
    {
        private ICommand _itemInvokedCommand;
        private object _selectedItem;

        public object SelectedItem
        {
            get => _selectedItem;
            set => Set(ref _selectedItem, value);
        }

        public ObservableCollection<FilePathViewModel> SourceData { get; } = new ObservableCollection<FilePathViewModel>();

        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<WinUI.TreeViewItemInvokedEventArgs>(OnItemInvoked));

        public FoldersViewModel()
        {
        }

        public async Task LoadDataAsync()
        {
            SourceData.Clear();

            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var response = await c.InternalResource.SendAsync(new LsInfoCommand("/"));

                if (response.IsResponseValid)
                    foreach (var item in response.Response.Content)
                    {
                        SourceData.Add(new FilePathViewModel(item));
                    }
            }
            OnPropertyChanged(nameof(SourceData));
        }

        private void OnItemInvoked(WinUI.TreeViewItemInvokedEventArgs args)
            => SelectedItem = args.InvokedItem;
    }
}
