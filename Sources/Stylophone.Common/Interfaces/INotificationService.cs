using System;
using System.Collections.Generic;
using System.Text;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.Interfaces
{
    public class InAppNotificationRequestedEventArgs : EventArgs { public string NotificationText { get; set; } public int NotificationTime { get; set; } }

    public interface INotificationService
    {
        void ShowBasicToastNotification(string title, string description);

        void ShowInAppNotification(string notification, bool autoHide = true);

        void ShowErrorNotification(Exception ex);
    }

    public abstract class NotificationServiceBase: INotificationService 
    {
        public event EventHandler<InAppNotificationRequestedEventArgs> InAppNotificationRequested;

        public void InvokeInAppNotificationRequested(InAppNotificationRequestedEventArgs args)
        {
            InAppNotificationRequested?.Invoke(this, args);
        }

        public void ShowErrorNotification(Exception ex) => ShowInAppNotification(string.Format(Resources.ErrorGeneric, ex), false);

        public abstract void ShowBasicToastNotification(string title, string description);
        public abstract void ShowInAppNotification(string notification, bool autoHide = true);
    }
}
