using Stylophone.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stylophone.Mobile.Services
{
    class NotificationService : NotificationServiceBase
    {
        public override void ShowBasicToastNotification(string title, string description)
        {
            // Not used by Stylophone at the moment.
            throw new NotImplementedException();
        }

        public override void ShowInAppNotification(string notification, bool autoHide = true)
        {
            InvokeInAppNotificationRequested(new InAppNotificationRequestedEventArgs { NotificationText = notification, NotificationTime = autoHide ? 1500 : 0 });
        }
    }
}
