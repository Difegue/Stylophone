using System;
using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    /// <summary>
    /// Shared layout logic for the album and playlist pages, whose blurred header extends under the navigation bar.
    /// </summary>
    public static class DetailHeaderHelper
    {
        /// <summary>
        /// Resize the table header to fit its content at the current width.
        /// </summary>
        public static void UpdateHeaderHeight(UITableView tableView)
        {
            var header = tableView.TableHeaderView;
            var width = tableView.Bounds.Width;
            if (header == null || width == 0)
                return;

            var height = header.SystemLayoutSizeFittingSize(new CGSize(width, UIView.UILayoutFittingCompressedSize.Height),
                (float)UILayoutPriority.Required, (float)UILayoutPriority.FittingSizeLevel).Height;

            if (header.Frame.Width == width && Math.Abs(header.Frame.Height - height) < 0.5)
                return;

            header.Frame = new CGRect(0, 0, width, height);
            // Reassign the header so the tableview picks up its new height
            tableView.TableHeaderView = header;
        }

        // Narrowest room beside sidebars for the regular header layout: art (32 + 196) and text (16 + 200 + 32)
        public const float RegularLayoutMinWidth = 480;

        /// <summary>
        /// Lay out the header's content beside sidebars, while its blurred album art keeps going underneath them.
        /// The storyboard positions the content relative to the margins of the header blur's content view.
        /// </summary>
        public static void FollowSafeArea(UIVisualEffectView headerBlur, UIView backgroundArt)
        {
            headerBlur.ContentView.InsetsLayoutMarginsFromSafeArea = true;
            headerBlur.ContentView.DirectionalLayoutMargins = NSDirectionalEdgeInsets.Zero;

            BackgroundExtensionHelper.ExtendUnderSidebars(backgroundArt);
        }

        /// <returns>True if the header is still visible under the navigation bar.</returns>
        private static bool IsHeaderUnderNavigationBar(UITableView tableView) =>
            // The safe area ends at the bar, unlike the content inset of headers raised into it (see VerticalBarHeaderLayout)
            tableView.ContentOffset.Y + tableView.SafeAreaInsets.Top < (tableView.TableHeaderView?.Frame.Height ?? 0);

        /// <returns>True once most of the given header view has scrolled past the top of the page's content
        /// (the navigation bar, or the top of headers raised into it).</returns>
        public static bool IsUnderNavigationBar(UITableView tableView, UIView headerView) =>
            headerView.ConvertRectToView(headerView.Bounds, tableView).GetMidY() < tableView.ContentOffset.Y + tableView.AdjustedContentInset.Top;

        /// <summary>
        /// Only show the bar's scroll edge effect once tracks scroll under it, so the blurred header looks continuous with the bar.
        /// </summary>
        public static void UpdateTopEdgeEffect(UITableView tableView)
        {
            if (OperatingSystem.IsIOSVersionAtLeast(26))
                tableView.TopEdgeEffect.Hidden = IsHeaderUnderNavigationBar(tableView);
        }

        // Distance over which the header's content fades out as it approaches the navigation bar
        private const float FadeLength = 16;

        /// <summary>
        /// Fade out the header's content as it scrolls under the navigation bar, so it doesn't clash with the bar's title and buttons.
        /// The blurred art behind it keeps extending under the bar.
        /// </summary>
        public static void UpdateHeaderFade(UITableView tableView, UIView headerContent)
        {
            // The header lays out after the tableview, make sure the content's size is up to date
            tableView.TableHeaderView?.LayoutIfNeeded();

            var height = headerContent.Bounds.Height;
            if (height == 0)
                return;

            if (headerContent.Layer.Mask is not CAGradientLayer fadeMask)
            {
                fadeMask = new CAGradientLayer { Colors = new[] { UIColor.Clear.CGColor, UIColor.Black.CGColor } };
                headerContent.Layer.Mask = fadeMask;
            }

            // Top of the page's content, in the header content's coordinates: the bottom of the navigation bar,
            // or above it for headers raised into the bar (see VerticalBarHeaderLayout)
            var barBottom = headerContent.ConvertPointFromView(
                new CGPoint(0, tableView.ContentOffset.Y + tableView.AdjustedContentInset.Top), tableView).Y;

            // Follow scrolling without implicit animations
            CATransaction.Begin();
            CATransaction.DisableActions = true;
            fadeMask.Frame = headerContent.Bounds;
            fadeMask.StartPoint = new CGPoint(0.5, barBottom / height);
            fadeMask.EndPoint = new CGPoint(0.5, (barBottom + FadeLength) / height);
            CATransaction.Commit();
        }
    }
}
