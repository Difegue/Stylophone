using System;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using Stylophone.Common.Interfaces;
using CommunityToolkit.Mvvm.Messaging;

namespace Stylophone.Services
{

    public class NotificationService : NotificationServiceBase
    {
        
        public override void ShowInAppNotification(InAppNotification notificationObject)
        {
            WeakReferenceMessenger.Default.Send(notificationObject);
        }
        

        public void ShowToastNotification(ToastNotification toastNotification)
        {
            try
            {
                ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
            }
            catch (Exception)
            {
                // TODO WTS: Adding ToastNotification can fail in rare conditions, please handle exceptions as appropriate to your scenario.
            }
        }

    }
}
