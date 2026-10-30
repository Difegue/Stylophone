using System;
using System.Linq;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    public static class BackgroundExtensionHelper
    {
        /// <summary>
        /// Lay out a background view (eg. album art) beside sidebars, and extend it underneath them,
        /// by moving it into a background extension view which takes its place and constraints.
        /// Does nothing on systems without background extension views, where the background keeps going under sidebars as is.
        /// </summary>
        public static void ExtendUnderSidebars(UIView background)
        {
            var superview = background.Superview;
            if (!OperatingSystem.IsIOSVersionAtLeast(26) || superview == null)
                return;

            var extension = new UIBackgroundExtensionView(CoreGraphics.CGRect.Empty)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                // Only follow the safe area horizontally: in scrollviews, its vertical insets change while scrolling
                AutomaticallyPlacesContentView = false
            };
            superview.InsertSubviewBelow(extension, background);

            // The extension view replaces the background in its superview's constraints
            var constraints = superview.Constraints.Where(c => c.FirstItem == background || c.SecondItem == background).ToArray();
            var extensionConstraints = constraints.Select(c =>
            {
                var constraint = NSLayoutConstraint.Create(c.FirstItem == background ? extension : c.FirstItem, c.FirstAttribute, c.Relation,
                    c.SecondItem == background ? extension : c.SecondItem, c.SecondAttribute, c.Multiplier, c.Constant);
                constraint.Priority = c.Priority;
                return constraint;
            }).ToArray();

            NSLayoutConstraint.DeactivateConstraints(constraints);
            background.RemoveFromSuperview();
            NSLayoutConstraint.ActivateConstraints(extensionConstraints);

            extension.ContentView = background;
            background.TranslatesAutoresizingMaskIntoConstraints = false;

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                background.TopAnchor.ConstraintEqualTo(extension.TopAnchor),
                background.BottomAnchor.ConstraintEqualTo(extension.BottomAnchor),
                background.LeadingAnchor.ConstraintEqualTo(extension.SafeAreaLayoutGuide.LeadingAnchor),
                background.TrailingAnchor.ConstraintEqualTo(extension.SafeAreaLayoutGuide.TrailingAnchor)
            });
        }
    }
}
