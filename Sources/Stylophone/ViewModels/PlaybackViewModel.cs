using Stylophone.Services;
using Stylophone.Views;
using Microsoft.Toolkit.Uwp;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using Stylophone.Localization.Strings;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace Stylophone.ViewModels
{
    public class PlaybackViewModel : PlaybackViewModelBase
    {
        public PlaybackViewModel(IDialogService dialogService, INavigationService navigationService, INotificationService notificationService, IDispatcherService dispatcherService, IInteropService interop, MPDConnectionService mpdService, TrackViewModelFactory trackVmFactory):
            base(dialogService,navigationService,notificationService,dispatcherService,interop,mpdService,trackVmFactory)
        {
            Application.Current.LeavingBackground += CurrentOnLeavingBackground;

            ((NavigationService)_navigationService).Navigated += (s, e) =>
                _dispatcherService.ExecuteOnUIThreadAsync(() => {
                    ShowTrackName = _navigationService.CurrentPageViewModelType != typeof(PlaybackViewModelBase);
                });
        }

        private void CurrentOnLeavingBackground(object sender, LeavingBackgroundEventArgs leavingBackgroundEventArgs)
        {
            // Refresh all
            UpdateInformation(sender, null);
        }

        public override void Dispose()
        {
            base.Dispose();

            Application.Current.LeavingBackground -= CurrentOnLeavingBackground;
        }

        private AppWindow _appWindow;
        /// <summary>
        /// Switch to compact overlay mode
        /// </summary>
        public override async Task SwitchToCompactViewAsync(EventArgs obj)
        {
            try
            {
                if (_appWindow == null)
                {
                    _appWindow = await AppWindow.TryCreateAsync();

                    // Create a new window within the view
                    Frame overlayFrame = new Frame();
                    overlayFrame.Navigate(typeof(OverlayView));

                    // Get a reference to the page instance and assign the
                    // newly created AppWindow to the HostAppWindow property.
                    OverlayView page = (OverlayView)overlayFrame.Content;
                    page.HostAppWindow = _appWindow;

                    // Set the window content
                    ElementCompositionPreview.SetAppWindowContent(_appWindow, overlayFrame);

                    // Make the window compact overlay
                    _appWindow.Presenter.RequestPresentation(AppWindowPresentationKind.CompactOverlay);
                    _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                    _appWindow.RequestSize(new Size(340, 364));

                    // When the window is closed, be sure to release
                    // XAML resources and the reference to the window.
                    _appWindow.Closed += delegate
                    {
                        overlayFrame.Content = null;
                        _appWindow = null;
                    };
                }

                await _appWindow.TryShowAsync();
            }
            catch (Exception e)
            {
                _notificationService.ShowInAppNotification(string.Format(Resources.ErrorGeneric, e), false);
            }
        }
    }
}
