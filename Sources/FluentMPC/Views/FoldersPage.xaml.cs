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
            if (args.Node.HasUnrealizedChildren)
            {
                var vm = (args.Node.Content as FilePathViewModel);
                Task.Run(async () => await vm.UpdateChildrenAsync());
                args.Node.HasUnrealizedChildren = false;
            }
        }
    }
}
