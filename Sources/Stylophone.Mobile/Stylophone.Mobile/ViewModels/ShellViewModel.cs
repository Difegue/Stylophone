using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;

namespace Stylophone.Mobile.ViewModels
{
    public class ShellViewModel : ShellViewModelBase
    {
        private Shell _appShell;

        public ShellViewModel(INavigationService navigationService, INotificationService notificationService, IDispatcherService dispatcherService, MPDConnectionService mpdService) :
            base(navigationService, notificationService, dispatcherService, mpdService)
        {
        }

        internal void Initialize(Shell appShell)
        {
            _appShell = appShell;


        }

        protected override void OnItemInvoked(object item)
        {
            throw new NotImplementedException();
        }

        protected override void OnLoaded()
        {
            throw new NotImplementedException();
        }

        protected override void ShowInAppNotification(object sender, InAppNotificationRequestedEventArgs e)
        {
            _appShell.DisplayToastAsync(e.NotificationText, e.NotificationTime);
        }

        protected override void UpdatePlaylistNavigation()
        {
            //throw new NotImplementedException();
        }
    }
}
