using System;
using UserNotifications;
using Stylophone.Common.Interfaces;
using UIKit;
using Xam.RMessage;

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
                RMessageType type = notification.NotificationType switch
                {
                    NotificationType.Info => RMessageType.Normal,
                    NotificationType.Warning => RMessageType.Warning,
                    NotificationType.Error => RMessageType.Error,
                    _ => RMessageType.Normal
                };

                RMessage.ShowNotificationWithTitle(notification.NotificationTitle, notification.NotificationText, type, "",
                    notification.NotificationType == NotificationType.Error ? 300000 : 2, () => { });
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
