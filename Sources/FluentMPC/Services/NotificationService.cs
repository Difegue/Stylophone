using System;
using System.Threading.Tasks;

using FluentMPC.Activation;

using Windows.ApplicationModel.Activation;
using Windows.UI.Notifications;
using Windows.UI.Xaml;

namespace FluentMPC.Services
{
    public class InAppNotificationRequestedEventArgs : EventArgs { public string NotificationText { get; set; } public int NotificationTime { get; set; } }

    internal partial class NotificationService : ActivationHandler<ToastNotificationActivatedEventArgs>
    {
        public static event EventHandler<InAppNotificationRequestedEventArgs> InAppNotificationRequested;

        public static void ShowInAppNotification(string notification, int time = 1500)
        {
            InAppNotificationRequested?.Invoke(Application.Current, new InAppNotificationRequestedEventArgs { NotificationText = notification, NotificationTime = time });
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

        protected override async Task HandleInternalAsync(ToastNotificationActivatedEventArgs args)
        {
            //// TODO WTS: Handle activation from toast notification
            //// More details at https://docs.microsoft.com/windows/uwp/design/shell/tiles-and-notifications/send-local-toast

            await Task.CompletedTask;
        }
    }
}
