using System;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    /// <summary>
    /// Opening a sidebar in an iOS app just changes the safe area, but some of our layout is dependent on size classes to work properly.
    /// This class switches a page to its compact width layout (the one of phones) when the room left beside sidebars is too narrow for its regular layout.
    /// 
    /// Storyboard size class variations follow the traits of their scene's view controller, so this overrides the page's horizontal size class.
    /// Changing traits invalidates layout, so call <see cref="Update"/> when the page appears, is resized, or its safe area changes.
    /// </summary>
    public class NarrowLayoutSwitch
    {
        private readonly UIViewController _viewController;
        private readonly nfloat _minRegularWidth;

        public NarrowLayoutSwitch(UIViewController viewController, nfloat minRegularWidth)
        {
            _viewController = viewController;
            _minRegularWidth = minRegularWidth;
        }

        public bool IsNarrow { get; private set; }

        /// <returns>Width of the page left beside sidebars.</returns>
        public static nfloat GetContentWidth(UIView view) =>
            view.Bounds.Width - view.SafeAreaInsets.Left - view.SafeAreaInsets.Right;

        public void Update()
        {
            var view = _viewController.ViewIfLoaded;
            if (!OperatingSystem.IsIOSVersionAtLeast(17) || view?.Window == null)
                return;

            // Our own traits may be overridden, so go by our parent's
            var traits = _viewController.ParentViewController?.TraitCollection ?? view.Window.TraitCollection;
            var isNarrow = traits.HorizontalSizeClass == UIUserInterfaceSizeClass.Regular && GetContentWidth(view) < _minRegularWidth;

            if (isNarrow == IsNarrow)
                return;

            IsNarrow = isNarrow;
            if (isNarrow)
                _viewController.TraitOverrides.HorizontalSizeClass = UIUserInterfaceSizeClass.Compact;
            else
                _viewController.TraitOverrides.RemoveTrait(typeof(UITraitHorizontalSizeClass));
        }
    }
}
