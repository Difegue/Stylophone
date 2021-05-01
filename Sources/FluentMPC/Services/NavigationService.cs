using FluentMPC.ViewModels;
using FluentMPC.Views;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Stylophone.Common.Interfaces;
using Stylophone.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentMPC.Services
{
    public class NavigationService : NavigationServiceBase
    {
        private Dictionary<Type, Type> _viewModelToPageDictionary = new Dictionary<Type, Type>()
        {
            { typeof(QueueViewModel), typeof(ServerQueuePage) },
            { typeof(SettingsViewModel), typeof(SettingsPage) },
            { typeof(AlbumDetailViewModel), typeof(LibraryDetailPage) },
            { typeof(PlaybackViewModelBase), typeof(PlaybackView) },
            { typeof(SearchResultsViewModel), typeof(SearchResultsPage) },
            // Technically unused, since handled directly by the NavigationView atm
            { typeof(FoldersViewModel), typeof(FoldersPage) },
            { typeof(PlaylistViewModel), typeof(PlaylistPage) },
            { typeof(LibraryViewModel), typeof(LibraryPage) }
        };

        public NavigationService()
        {

        }

        public override bool CanGoBack => Frame.CanGoBack;

        public override Type CurrentPageViewModelType => _viewModelToPageDictionary.Keys.Where(
            k => _viewModelToPageDictionary[k] == Frame.CurrentSourcePageType).FirstOrDefault();

        private object _lastParamUsed;
        public override void NavigateImplementation(Type viewmodelType, object parameter = null)
        {
            // Get the matching page and navigate to it
            var pageType = _viewModelToPageDictionary.GetValueOrDefault(viewmodelType);

            // Don't open the same page multiple times
            if (Frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParamUsed)))
            {
                var navigationResult = Frame.Navigate(pageType, parameter);
                if (navigationResult)
                {
                    _lastParamUsed = parameter;
                }
            }
        }

        public override void SetListDataItemForNextConnectedAnimation(object item) => Frame.SetListDataItemForNextConnectedAnimation(item);

        public override bool GoBackImplementation()
        {
            if (CanGoBack)
            {
                Frame.GoBack();
                return true;
            }

            return false;
        }

        private Frame _frame;
        public Frame Frame
        {
            get
            {
                if (_frame == null)
                {
                    _frame = Window.Current.Content as Frame;
                }

                return _frame;
            }

            set
            {
                _frame = value;
            }
        }

    }
}
