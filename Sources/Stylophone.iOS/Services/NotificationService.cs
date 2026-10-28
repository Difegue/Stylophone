using System;
using UserNotifications;
using CommunityToolkit.Mvvm.DependencyInjection;
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
                UIApplication.SharedApplication.InvokeOnMainThread(() => ShowErrorAlert(notification));
                return;
            }

            var rootVc = (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController;

            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                // The main window might not be around if only separate album/playlist windows are open
                if (UIApplication.SharedApplication.ApplicationState != UIApplicationState.Active || rootVc?.View.Window == null)
                    return;

                // Pop out of the compact player, or the bottom of the page without it
                var navigationController = (Ioc.Default.GetRequiredService<INavigationService>() as NavigationService)?.NavigationController as NavigationController;
                var (sourceView, sourceRect) = navigationController?.GetNotificationSource() ??
                    (rootVc.View, NotificationPopoverViewController.GetPageBottom(rootVc.View));

                var popover = new NotificationPopoverViewController(notification.NotificationTitle, sourceView, sourceRect);

                rootVc.PresentViewController(popover, true, null);
            });

            _notificationTimer?.Dispose();
            _notificationTimer = new Timer((_) => UIApplication.SharedApplication.InvokeOnMainThread(() =>
                        rootVc?.DismissViewController(true, null)),
                    null, 2000, Timeout.Infinite);
        }

        private static void ShowErrorAlert(InAppNotification notification)
        {
            // UIAlertView isn't available to UIScene based apps, so present an alert controller over whichever window is in front
            var presenter = InteropService.GetPresentingViewController();
            if (presenter == null || presenter.IsBeingDismissed)
                return;

            var alert = UIAlertController.Create(notification.NotificationTitle, notification.NotificationText, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
            presenter.PresentViewController(alert, true, null);
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
        /// <summary>
        /// Distance between the tip of notifications and what they point at.
        /// </summary>
        public const float SourceGap = 8;

        // Room around the text, and the narrowest notifications get
        private const float HorizontalPadding = 20;
        private const float VerticalPadding = 16;
        private const float MinWidth = 196;
        // Popovers keep off the edges of the screen
        private const float ScreenMargin = 16;

        private string _text;
        private nfloat _maxWidth;

        /// <param name="sourceView">View the popover comes out of, from above it</param>
        /// <param name="sourceRect">Area of the source view to point at, from the middle of its top edge.
        /// The popover can get as wide as it for longer texts.</param>
        public NotificationPopoverViewController(string text, UIView sourceView, CGRect sourceRect)
        {
            _text = text;
            var windowWidth = sourceView.Window?.Bounds.Width ?? sourceRect.Width;
            _maxWidth = nfloat.Max(MinWidth, nfloat.Min(sourceRect.Width, windowWidth - 2 * ScreenMargin));

            ModalPresentationStyle = UIModalPresentationStyle.Popover;

            PopoverPresentationController.SourceView = sourceView;
            PopoverPresentationController.Delegate = this;
            PopoverPresentationController.PermittedArrowDirections = UIPopoverArrowDirection.Down;
            PopoverPresentationController.SourceRect = sourceRect;
        }

        /// <returns>Area to point notifications at along the bottom of a page (its safe area keeps clear of sidebars and toolbars).</returns>
        public static CGRect GetPageBottom(UIView page)
        {
            var safeArea = page.SafeAreaLayoutGuide.LayoutFrame.Inset(16, 0);
            return new CGRect(safeArea.X, safeArea.Bottom - SourceGap, safeArea.Width, 0);
        }

        public override void LoadView()
        {
            base.LoadView();

            var label = new UILabel
            {
                Text = _text,
                Font = UIFont.PreferredHeadline,
                TextAlignment = UITextAlignment.Center,
                AdjustsFontSizeToFitWidth = true,
                Lines = 2,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(label);

            // The safe area leaves out the arrow
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                label.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, HorizontalPadding),
                label.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -HorizontalPadding),
                label.CenterYAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.CenterYAnchor)
            });

            // Keep the text on one line where it fits, wrapping it once it'd get wider than what the popover points at
            var maxTextWidth = _maxWidth - 2 * HorizontalPadding;
            var textWidth = nfloat.Min(label.SizeThatFits(new CGSize(10000, 10000)).Width, maxTextWidth);
            var textHeight = label.SizeThatFits(new CGSize(textWidth, 10000)).Height;
            PreferredContentSize = new CGSize(nfloat.Max(MinWidth, textWidth + 2 * HorizontalPadding), textHeight + 2 * VerticalPadding);
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
