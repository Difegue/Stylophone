using System;
using UserNotifications;
using Stylophone.Common.Interfaces;

namespace Stylophone.iOS.Services
{
    public class NotificationService : NotificationServiceBase
    {
        private IDispatcherService _dispatcherService;
        private Timer _notificationTimer;

        public NotificationService(IDispatcherService dispatcherService)
        {
            _dispatcherService = dispatcherService;
        }

        public override void ShowInAppNotification(InAppNotification notification)
        {
            if (notification.NotificationType == NotificationType.Error)
            {
                // Let's just use alerts until TipKit is available... This is cheap but w/e
                var alert = new UIAlertView(notification.NotificationTitle, notification.NotificationText, null, "Ok");
                UIApplication.SharedApplication.InvokeOnMainThread(() => alert.Show());
                return;
            }

            var rootVc = (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController;

            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                if (UIApplication.SharedApplication.ApplicationState != UIApplicationState.Active)
                    return;

                var popover = new NotificationPopoverViewController(notification.NotificationTitle, rootVc);

                rootVc.PresentViewController(popover, true, null);
            });

            _notificationTimer?.Dispose();
            _notificationTimer = new Timer((_) => UIApplication.SharedApplication.InvokeOnMainThread(() =>
                        rootVc.DismissViewController(true, null)),
                    null, 2000, Timeout.Infinite);
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

    public class NotificationPopoverViewController : UIViewController, IUIPopoverPresentationControllerDelegate
    {
        private string _text;

        public NotificationPopoverViewController(string text, UISplitViewController rootVc) //, CGRect sourceBounds, CGSize contentSize)
        {
            _text = text;

            ModalPresentationStyle = UIModalPresentationStyle.Popover;

            PopoverPresentationController.SourceView = rootVc.View;
            PopoverPresentationController.Delegate = this;
            PopoverPresentationController.PermittedArrowDirections = UIPopoverArrowDirection.Left;
            PopoverPresentationController.SourceRect = new CGRect(16, 64, 1, 1);
        }

        public override void LoadView()
        {
            base.LoadView();

            PreferredContentSize = new CGSize(196, 54);

            var stackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Horizontal,
                Distribution = UIStackViewDistribution.FillProportionally,
                Spacing = 32
            };

            stackView.AddArrangedSubview(new UIView());
            stackView.AddArrangedSubview(new UILabel
            {
                Text = _text,
                Font = UIFont.PreferredHeadline,
                AdjustsFontSizeToFitWidth = true,
                Lines = 2,
            });
            stackView.AddArrangedSubview(new UIView());

            View = stackView;

        }

        [Export("adaptivePresentationStyleForPresentationController:traitCollection:")]
        public UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller, UITraitCollection traitCollection)
        {
            // Prevent popover from being adaptive fullscreen on phones
            // (https://pspdfkit.com/blog/2022/presenting-popovers-on-iphone-with-swiftui/)
            return UIModalPresentationStyle.None;
        }

    }

}
