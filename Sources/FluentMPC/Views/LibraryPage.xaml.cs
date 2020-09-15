using System;
using System.Threading.Tasks;
using FluentMPC.ViewModels;
using FluentMPC.ViewModels.Items;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentMPC.Views
{
    public sealed partial class LibraryPage : Page
    {
        public LibraryViewModel ViewModel { get; } = new LibraryViewModel();

        public LibraryPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ViewModel.Source.Count == 0)
                await ViewModel.LoadDataAsync();
        }

        private void AlbumClicked(object sender, ItemClickEventArgs e)
        {
            ViewModel.ItemClickCommand.Execute(e.ClickedItem);
        }

        private void OnFilterChanged(object sender, TextChangedEventArgs e)
        {
            var searchTerms = (sender as TextBox).Text;

            if (searchTerms.Length < 3)
                searchTerms = "";

            ViewModel.FilterLibrary(searchTerms);
        }

    }
}
