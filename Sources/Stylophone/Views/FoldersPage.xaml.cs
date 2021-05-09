using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Platforms.Uap.Views;
using MvvmCross.ViewModels;
using Stylophone.Common.Interfaces;
using Stylophone.Common.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Stylophone.Views
{
    [MvxViewFor(typeof(FoldersViewModel))]
    public sealed partial class FoldersPage : MvxWindowsPage
    {
        public FoldersViewModel Vm => (FoldersViewModel)ViewModel;

        public FoldersPage()
        {
            InitializeComponent();
        }

        protected override async void OnViewModelSet()
        {
            await Vm.LoadDataAsync();
        }

        private void treeView_Expanding(Microsoft.UI.Xaml.Controls.TreeView sender, Microsoft.UI.Xaml.Controls.TreeViewExpandingEventArgs args)
        {
            var vm = (args.Item as FilePathViewModel);

            if (!vm.IsLoaded)
            {
                Task.Run(async () =>
                {
                await vm.LoadChildrenAsync();
                await Mvx.IoCProvider.Resolve<IDispatcherService>().ExecuteOnUIThreadAsync(() => args.Node.HasUnrealizedChildren = false);
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
