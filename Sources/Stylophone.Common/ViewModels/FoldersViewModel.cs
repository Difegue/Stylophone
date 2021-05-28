using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using MpcNET.Commands.Database;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.ViewModels
{
    public class FoldersViewModel : ViewModelBase
    {
        private FilePathViewModelFactory _fileVmFactory;
        private MPDConnectionService _mpdService;

        public FoldersViewModel(FilePathViewModelFactory fileVmFactory, MPDConnectionService mpdService, IDispatcherService dispatcherService): base(dispatcherService)
        {
            _mpdService = mpdService;
            _fileVmFactory = fileVmFactory;

            SourceData.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));
        }

        public static new string GetHeader() => Resources.FoldersHeader;

        public ObservableCollection<FilePathViewModel> SourceData { get; } = new ObservableCollection<FilePathViewModel>();
        public bool IsSourceEmpty => SourceData.Count == 0;

        public async Task LoadDataAsync()
        {
            SourceData.Clear();

            var response = await _mpdService.SafelySendCommandAsync(new LsInfoCommand("/"));

            if (response != null)
                foreach (var item in response)
                {
                    SourceData.Add(_fileVmFactory.GetFilePathViewModel(item, null));
                }
                
            OnPropertyChanged(nameof(SourceData));
        }

    }
}
