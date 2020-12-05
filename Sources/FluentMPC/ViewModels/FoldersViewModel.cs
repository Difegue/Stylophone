using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

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
        public ObservableCollection<FilePathViewModel> SourceData { get; } = new ObservableCollection<FilePathViewModel>();
        public bool IsSourceEmpty => SourceData.Count == 0;

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

    }
}
