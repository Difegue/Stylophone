using System;
using System.Threading.Tasks;
using FluentMPC.ViewModels;
using FluentMPC.ViewModels.Items;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentMPC.Views
{
    // For more info about the TreeView Control see
    // https://docs.microsoft.com/windows/uwp/design/controls-and-patterns/tree-view
    // For other samples, get the XAML Controls Gallery app http://aka.ms/XamlControlsGallery
    public sealed partial class FoldersPage : Page
    {
        public FoldersViewModel ViewModel { get; } = new FoldersViewModel();

        public FoldersPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadDataAsync();
        }

        private void treeView_Expanding(Microsoft.UI.Xaml.Controls.TreeView sender, Microsoft.UI.Xaml.Controls.TreeViewExpandingEventArgs args)
        {
            var vm = (args.Item as FilePathViewModel);

            if (!vm.IsLoaded)
            {
                Task.Run(async () =>
                {
                    await vm.LoadChildrenAsync();
                    args.Node.HasUnrealizedChildren = false;
                });
            }       
        }

        private void TreeViewItem_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            var treeViewItem = sender as Microsoft.UI.Xaml.Controls.TreeViewItem;
            var fileVm = treeViewItem.DataContext as FilePathViewModel;
            fileVm.PlayCommand.Execute(null);
        }
    }
}
