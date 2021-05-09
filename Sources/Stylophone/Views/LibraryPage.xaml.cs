using Stylophone.ViewModels;
using MvvmCross;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MvvmCross.Platforms.Uap.Views;
using MvvmCross.ViewModels;

namespace Stylophone.Views
{
    [MvxViewFor(typeof(LibraryViewModel))]
    public sealed partial class LibraryPage : MvxWindowsPage
    {
        public LibraryViewModel Vm => (LibraryViewModel)ViewModel;

        public LibraryPage()
        {
            InitializeComponent();
        }

        protected override async void OnViewModelSet()
        {
            if (Vm.Source.Count == 0)
                await Vm.LoadDataAsync();
        }

        private void AlbumClicked(object sender, ItemClickEventArgs e)
        {
            Vm.ItemClickCommand.Execute(e.ClickedItem);
        }

        private void OnFilterChanged(object sender, TextChangedEventArgs e)
        {
            var searchTerms = (sender as TextBox).Text;

            if (searchTerms.Length < 3)
                searchTerms = "";

            Vm.FilterLibrary(searchTerms);
        }

    }
}
