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

        public override void ShowInAppNotification(string notification, bool autoHide)
        {
            if (autoHide)
                RMessage.ShowNotificationWithTitle(notification, "", RMessageType.Normal, "", 2, () => { }, true);
            else
                RMessage.ShowNotificationWithTitle(notification, RMessageType.Error, "", () => { });
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
