using System;
using CoreGraphics;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    /// <summary>
    /// Navigation bar title view for a label whose text changes while it's shown.
    /// Bars only measure their title view when it's set, so this takes all the room the bar gives it and fits the label within, 
    /// centering the text when possible.
    /// </summary>
    public class FlexibleTitleView : UIView
    {
        public UILabel Label { get; } = new UILabel();

        public FlexibleTitleView()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(Label);
        }

        // Fill the room between the bar's buttons, with the height of all the label's lines
        public override CGSize IntrinsicContentSize =>
            new CGSize(UILayoutFittingExpandedSize.Width, nfloat.Ceiling(Label.Font.LineHeight * nfloat.Max(1, Label.Lines)));

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            // Dynamic Type changes the label's font
            if (previousTraitCollection?.PreferredContentSizeCategory != TraitCollection.PreferredContentSizeCategory)
                InvalidateIntrinsicContentSize();
        }

        public string Text
        {
            get => Label.Text;
            set
            {
                Label.Text = value;
                SetNeedsLayout();
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var size = Label.SizeThatFits(Bounds.Size);
            var width = nfloat.Min(size.Width, Bounds.Width);
            var height = nfloat.Min(size.Height, Bounds.Height);

            // The room between the bar's buttons isn't centered when they differ on each side: center on the bar instead, like its titles.
            // Bars can extend under sidebars and vertical bars, which their safe area leaves out.
            var centerX = Bounds.GetMidX();
            for (var view = Superview; view != null; view = view.Superview)
            {
                if (view is UINavigationBar bar)
                {
                    var insets = bar.SafeAreaInsets;
                    var barCenterX = bar.Bounds.GetMinX() + insets.Left + (bar.Bounds.Width - insets.Left - insets.Right) / 2;
                    centerX = ConvertPointFromView(new CGPoint(barCenterX, 0), bar).X;
                    break;
                }
            }

            var x = nfloat.Max(0, nfloat.Min(centerX - width / 2, Bounds.Width - width));
            Label.Frame = new CGRect(x, (Bounds.Height - height) / 2, width, height);
        }
    }
}
