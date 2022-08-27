using System;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using Stylophone.Common.Interfaces;
using CommunityToolkit.Mvvm.Messaging;

namespace Stylophone.Services
{

    public class InAppNotification { public string NotificationText { get; set; } public bool AutoHide { get; set; } }

    public class NotificationService : NotificationServiceBase
    {


        public override void ShowInAppNotification(string notification, bool autoHide)
        {
            var notificationObject = new InAppNotification { NotificationText = notification, AutoHide = autoHide};
            WeakReferenceMessenger.Default.Send(notificationObject);
        }
        
        public override void ShowBasicToastNotification(string title, string description)
        {
            // Create the toast content
            var content = new ToastContent()
            {
                // More about the Launch property at https://docs.microsoft.com/dotnet/api/microsoft.toolkit.uwp.notifications.toastcontent
                Launch = "ToastContentActivationParams",

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title
                            },

                            new AdaptiveText()
                            {
                                 Text = description
                            }
                        }
                    }
                },

                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        // More about Toast Buttons at https://docs.microsoft.com/dotnet/api/microsoft.toolkit.uwp.notifications.toastbutton
                        new ToastButton("OK", "ToastButtonActivationArguments")
                        {
                            ActivationType = ToastActivationType.Foreground
                        },

                        new ToastButtonDismiss("Cancel")
                    }
                }
            };

            // Add the content to the toast
            var toast = new ToastNotification(content.GetXml())
            {
                // TODO WTS: Set a unique identifier for this notification within the notification group. (optional)
                // More details at https://docs.microsoft.com/uwp/api/windows.ui.notifications.toastnotification.tag
                Tag = "ToastTag"
            };

            // And show the toast
            ShowToastNotification(toast);
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
