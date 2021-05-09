using MvvmCross;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Uap.Presenters;
using MvvmCross.Platforms.Uap.Presenters.Attributes;
using MvvmCross.Platforms.Uap.Views;
using MvvmCross.Presenters;
using MvvmCross.Presenters.Attributes;
using MvvmCross.Presenters.Hints;
using MvvmCross.ViewModels;
using Stylophone.Common.ViewModels;
using Stylophone.ViewModels;
using Stylophone.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Stylophone
{
    public class StylophoneViewPresenter : IMvxWindowsViewPresenter
    {
        private Dictionary<Type, Type> _viewModelToPageDictionary = new Dictionary<Type, Type>()
        {
            { typeof(QueueViewModel), typeof(ServerQueuePage) },
            { typeof(SettingsViewModel), typeof(SettingsPage) },
            { typeof(AlbumDetailViewModel), typeof(LibraryDetailPage) },
            { typeof(PlaybackViewModel), typeof(PlaybackView) },
            { typeof(SearchResultsViewModel), typeof(SearchResultsPage) },
            { typeof(FoldersViewModel), typeof(FoldersPage) },
            { typeof(PlaylistViewModel), typeof(PlaylistPage) },
            { typeof(LibraryViewModel), typeof(LibraryPage) }
        };

        IMvxWindowsFrame _rootFrame;
        Frame _shellFrame;

        public StylophoneViewPresenter(IMvxWindowsFrame rootFrame)
        {
            _rootFrame = rootFrame;

        }

        public Task<bool> Close(IMvxViewModel viewModel)
        {
            if (_shellFrame.CanGoBack)
            {
                _shellFrame.GoBack();
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        private object _lastParamUsed;
        public Task<bool> Show(MvxViewModelRequest request)
        {
            var requestText = GetRequestText(request);

            var viewModel = request.ViewModelType;

            if (viewModel == typeof(ShellViewModel))
            {
                // Initial request, set rootFrame to Shell and register its container frame for other viewmodels
                _rootFrame.Navigate(typeof(ShellPage), requestText); //Frame won't allow serialization of it's nav-state if it gets a non-simple type as a nav param

                if (_rootFrame.Content is ShellPage page)
                {
                    _shellFrame = ((ShellViewModel)page.ViewModel).GetShellFrame();
                }

                // Chain with loading QueueViewModel
                Mvx.IoCProvider.Resolve<IMvxNavigationService>().Navigate<QueueViewModel>();
                return Task.FromResult(true);
            }

            // Get the matching page and navigate to it
            var pageType = _viewModelToPageDictionary.GetValueOrDefault(viewModel);
            var shellType = _shellFrame.Content?.GetType();

            // Don't open the same page multiple times
            var parameter = request.ParameterValues?.Values.FirstOrDefault();
            if (shellType != pageType || (parameter != null && !parameter.Equals(_lastParamUsed)))
            {
                var navigationResult = _shellFrame.Navigate(pageType, requestText);
                if (navigationResult)
                {
                    _lastParamUsed = parameter;
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        Task<bool> IMvxViewPresenter.ChangePresentation(MvxPresentationHint hint)
        {
            if (hint is MvxClosePresentationHint closeHint)
            {
                return Close(closeHint.ViewModelToClose);
            }

            return Task.FromResult(false);
        }

        public void AddPresentationHintHandler<THint>(Func<THint, Task<bool>> action) where THint : MvxPresentationHint
        {
            throw new NotImplementedException();
        }

        protected virtual string GetRequestText(MvxViewModelRequest request)
        {
            var requestTranslator = Mvx.IoCProvider.Resolve<IMvxWindowsViewModelRequestTranslator>();
            string requestText = string.Empty;
            if (request is MvxViewModelInstanceRequest)
            {
                requestText = requestTranslator.GetRequestTextWithKeyFor(((MvxViewModelInstanceRequest)request).ViewModelInstance);
            }
            else
            {
                requestText = requestTranslator.GetRequestTextFor(request);
            }

            return requestText;
        }
    }

    public class ShellFrameHint : MvxPresentationHint
    {
        public Frame RootFrame { get; set; }

        public ShellFrameHint(Frame frame)
        {
            RootFrame = frame;
        }
    }
}
