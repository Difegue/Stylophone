using System;
using CoreGraphics;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    /// <summary>
    /// The inner screen of foldable iPhones moves bar buttons to a vertical bar on its side, leaving the navigation bar of pages mostly empty.
    /// Raises the header of album and playlist pages into it there: the art goes level with the buttons left in the bar
    /// (past the sidebar button on its leading edge), while the labels stay below the bar, which can still have buttons above them.
    /// Call <see cref="Update"/> on layout, before fitting the header.
    /// </summary>
    public class VerticalBarHeaderLayout
    {
        // Top of the header's content, below the top of the header (see the storyboards)
        private const float ContentTop = 16;
        // Least distance between the art and the leading layout margin of the header (see the storyboards)
        private const float ArtLeading = 32;
        // Leading edge of the art past the sidebar button, which the bar shows on its own leading edge while the sidebar is hidden
        private const float ArtLeadingPastSidebarButton = 85;

        private readonly UITableViewController _viewController;
        private readonly UIVisualEffectView _headerBlur;
        private readonly NSLayoutConstraint _labelsBelowBar;
        private readonly NSLayoutConstraint _artAtLeadingMargin;
        private nfloat _leadingMargin;

        /// <param name="headerBlur">The header's blur, whose content view's margins lay out the header</param>
        /// <param name="art">The header's art, along its leading edge</param>
        /// <param name="topLabel">The header's label placed above the others</param>
        public VerticalBarHeaderLayout(UITableViewController viewController, UIVisualEffectView headerBlur, UIView art, UIView topLabel)
        {
            _viewController = viewController;
            _headerBlur = headerBlur;
            _labelsBelowBar = topLabel.TopAnchor.ConstraintGreaterThanOrEqualTo(viewController.TableView.TableHeaderView.TopAnchor);

            // The art can otherwise drift further from the margin as the labels hug their text: keep it close to the sidebar button
            _artAtLeadingMargin = art.LeadingAnchor.ConstraintEqualTo(headerBlur.ContentView.LayoutMarginsGuide.LeadingAnchor, ArtLeading);
            _artAtLeadingMargin.Priority = (float)UILayoutPriority.DefaultLow + 10;
        }

        public void Update()
        {
            var tableView = _viewController.TableView;
            var bar = _viewController.NavigationController?.NavigationBar;
            if (bar == null || tableView.Window == null)
                return;

            // Only the regular layout of the header has the art next to its labels (folded iPhones get the layout of phones)
            nfloat raise = 0, leadingMargin = 0;
            if (VerticalBarHelper.HasVerticalBar(tableView.Window.TraitCollection) &&
                _viewController.TraitCollection.HorizontalSizeClass == UIUserInterfaceSizeClass.Regular)
            {
                // The safe area ends at the bottom of the bar: put the top of the art level with the top of the bar
                raise = nfloat.Max(0, tableView.SafeAreaInsets.Top + ContentTop - bar.Frame.Top);

                if (_viewController.SplitViewController?.DisplayMode == UISplitViewControllerDisplayMode.SecondaryOnly)
                    leadingMargin = ArtLeadingPastSidebarButton - ArtLeading;
            }

            // The bottom of the bar is as far below the top of the raised header as the header was raised
            if (_labelsBelowBar.Constant != raise)
                _labelsBelowBar.Constant = raise;
            if (_labelsBelowBar.Active != raise > 0)
                _labelsBelowBar.Active = _artAtLeadingMargin.Active = raise > 0;

            // The margins add up with the safe area, which reading them includes: set them whole (see DetailHeaderHelper.FollowSafeArea)
            if (_leadingMargin != leadingMargin)
            {
                _leadingMargin = leadingMargin;
                _headerBlur.ContentView.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(0, leadingMargin, 0, 0);
            }

            var inset = tableView.ContentInset;
            if (inset.Top != -raise)
            {
                // Pages scrolled to their top stay there
                var isAtTop = tableView.ContentOffset.Y <= 0.5 - tableView.AdjustedContentInset.Top;
                tableView.ContentInset = new UIEdgeInsets(-raise, inset.Left, inset.Bottom, inset.Right);
                if (isAtTop)
                    tableView.ContentOffset = new CGPoint(tableView.ContentOffset.X, -tableView.AdjustedContentInset.Top);
            }
        }
    }
}
