using System;
using System.Threading.Tasks;

using Microsoft.Toolkit.Uwp.Notifications;
using Stylophone.Common.Interfaces;
using Windows.ApplicationModel.Activation;
using Windows.UI.Notifications;

namespace Stylophone.Services
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
            //TODO: check for compact mode
            InvokeInAppNotificationRequested(new InAppNotificationRequestedEventArgs { NotificationText = notification, NotificationTime = autoHide ? 1500 : 0});
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
