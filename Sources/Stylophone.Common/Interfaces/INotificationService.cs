using System;
using System.Collections.Generic;
using System.Text;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.Interfaces
{
    public class InAppNotification 
    { 
        public string NotificationTitle { get; set; }
        public string NotificationText { get; set; } 
        public NotificationType NotificationType { get; set; }
    }

    public enum NotificationType
    {
        Info,
        Warning,
        Error
    }

    public interface INotificationService
    {
        void ShowBasicToastNotification(string title, string description);

        void ShowInAppNotification(string text, string description = "", NotificationType type = NotificationType.Info);
        void ShowInAppNotification(InAppNotification notification);

        void ShowErrorNotification(Exception ex);
    }

    public abstract class NotificationServiceBase: INotificationService 
    {
        public void ShowInAppNotification(string text, string description = "", NotificationType type = NotificationType.Info)
        {
            var notification = new InAppNotification
            {
                NotificationTitle = text,
                NotificationText = description,
                NotificationType = type
            };
            ShowInAppNotification(notification);
        }

        public void ShowErrorNotification(Exception ex)
        {
            var notification = new InAppNotification
            {
                NotificationTitle = string.Format(Resources.ErrorGeneric, ex.Message),
                NotificationText = ex.StackTrace,
                NotificationType = NotificationType.Error
            };
            ShowInAppNotification(notification);
        }
        
        public abstract void ShowBasicToastNotification(string title, string description);
        public abstract void ShowInAppNotification(InAppNotification notification);
    }
}
