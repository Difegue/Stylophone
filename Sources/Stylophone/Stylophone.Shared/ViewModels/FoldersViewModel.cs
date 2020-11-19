using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Stylophone.Helpers;
using Stylophone.Services;
using Stylophone.ViewModels.Items;
using MpcNET.Commands.Database;
using MpcNET.Types;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace Stylophone.ViewModels
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
        public bool IsSourceEmpty => SourceData.Count == 0;

        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<WinUI.TreeViewItemInvokedEventArgs>(OnItemInvoked));

        public FoldersViewModel()
        {
            SourceData.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));
        }

        public async Task LoadDataAsync()
        {
            SourceData.Clear();

            var response = await MPDConnectionService.SafelySendCommandAsync(new LsInfoCommand("/"));

            if (response != null)
                foreach (var item in response)
                {
                    SourceData.Add(new FilePathViewModel(item));
                }
                
            OnPropertyChanged(nameof(SourceData));
        }

        private void OnItemInvoked(WinUI.TreeViewItemInvokedEventArgs args)
            => SelectedItem = args.InvokedItem;
    }
}
