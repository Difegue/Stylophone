using Stylophone.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace Stylophone.Mobile.Services
{
    public class NavigationService : NavigationServiceBase
    {
        private Dictionary<Type, Type> _viewModelToPageDictionary = new Dictionary<Type, Type>()
        {
            /*{ typeof(QueueViewModel), typeof(ServerQueuePage) },
            { typeof(SettingsViewModel), typeof(SettingsPage) },
            { typeof(AlbumDetailViewModel), typeof(LibraryDetailPage) },
            { typeof(PlaybackViewModelBase), typeof(PlaybackView) },
            { typeof(SearchResultsViewModel), typeof(SearchResultsPage) },
            // Technically unused, since handled directly by the NavigationView atm
            { typeof(FoldersViewModel), typeof(FoldersPage) },
            { typeof(PlaylistViewModel), typeof(PlaylistPage) },
            { typeof(LibraryViewModel), typeof(LibraryPage) }*/
        };

        public NavigationService()
        {

        }

        public override bool CanGoBack => true; 

        public override Type CurrentPageViewModelType => _viewModelToPageDictionary.Keys.Where(
            k => _viewModelToPageDictionary[k] == Shell.Current.CurrentPage.GetType()).FirstOrDefault();

        public override bool GoBackImplementation() => Shell.Current.SendBackButtonPressed();

        public override void NavigateImplementation(Type viewmodel, object parameter = null)
        {
            Shell.Current.GoToAsync("//LoginPage");
        }

        public override void SetListDataItemForNextConnectedAnimation(object item)
        {
            throw new NotImplementedException();
        }
    }
}
