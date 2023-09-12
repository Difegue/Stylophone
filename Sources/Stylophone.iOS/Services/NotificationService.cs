using System;
using UserNotifications;
using Stylophone.Common.Interfaces;
using UIKit;

namespace Stylophone.iOS.Services
{
    public class NotificationService : NotificationServiceBase
    {
        private IDispatcherService _dispatcherService;

        public NotificationService(IDispatcherService dispatcherService)
        {
            _dispatcherService = dispatcherService;
        }

        public override void ShowInAppNotification(InAppNotification notification)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                if (UIApplication.SharedApplication.ApplicationState != UIApplicationState.Active)
                    return;

                // Let's just use alerts until TipKit is available... This is cheap but w/e
                var alert = new UIAlertView(notification.NotificationTitle, notification.NotificationText, null, "Ok");

                if (notification.NotificationType == NotificationType.Error) 
                    alert.Show();

            });
        }

        public override void ShowBasicToastNotification(string title, string description)
        {
            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Provisional, (res, err) =>
            {
                if (err == null)
                {
                    var content = new UNMutableNotificationContent();
                    content.Title = title;
                    content.Body = description;

                    var request = UNNotificationRequest.FromIdentifier("noti", content, null);
                    UNUserNotificationCenter.Current.AddNotificationRequest(request, null);
                }
            });
        }
    }
}
