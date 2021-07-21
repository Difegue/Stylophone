using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Stylophone.Common.Interfaces;
using Stylophone.Common.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Stylophone.Views
{
    // For more info about the TreeView Control see
    // https://docs.microsoft.com/windows/uwp/design/controls-and-patterns/tree-view
    // For other samples, get the XAML Controls Gallery app http://aka.ms/XamlControlsGallery
    public sealed partial class FoldersPage : Page
    {
        public FoldersViewModel ViewModel => (FoldersViewModel)DataContext;

        public FoldersPage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<FoldersViewModel>();
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
                await Ioc.Default.GetRequiredService<IDispatcherService>().ExecuteOnUIThreadAsync(() => args.Node.HasUnrealizedChildren = false);
                }).ConfigureAwait(false);
            }       
        }

        private void TreeViewItem_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            var treeViewItem = sender as Microsoft.UI.Xaml.Controls.TreeViewItem;
            var fileVm = treeViewItem.DataContext as FilePathViewModel;
            fileVm.AddToQueueCommand.Execute(null);
        }

    }
}
