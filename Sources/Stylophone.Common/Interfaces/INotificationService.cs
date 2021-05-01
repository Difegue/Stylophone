using System;
using System.Collections.Generic;
using System.Text;

namespace Stylophone.Common.Interfaces
{
    public class InAppNotificationRequestedEventArgs : EventArgs { public string NotificationText { get; set; } public int NotificationTime { get; set; } }

    public interface INotificationService
    {
        void ShowBasicToastNotification(string title, string description);

        void ShowInAppNotification(string notification, bool autoHide = true);
    }

    public abstract class NotificationServiceBase: INotificationService 
    {
        public event EventHandler<InAppNotificationRequestedEventArgs> InAppNotificationRequested;

        public void InvokeInAppNotificationRequested(InAppNotificationRequestedEventArgs args)
        {
            InAppNotificationRequested?.Invoke(this, args);
        }

        public abstract void ShowBasicToastNotification(string title, string description);
        public abstract void ShowInAppNotification(string notification, bool autoHide = true);
    }
}
