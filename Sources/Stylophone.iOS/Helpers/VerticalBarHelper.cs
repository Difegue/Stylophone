using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    /// <summary>
    /// Since iOS 27.1, some screens (like the ones of foldable iPhones) get their navigation and toolbar items in a vertical bar on their side.
    /// </summary>
    public static class VerticalBarHelper
    {
        /// <returns>True where the system places bars in a vertical bar on the side of the screen.</returns>
        public static bool HasVerticalBar(UITraitCollection traits)
        {
            // Not in the bindings yet: read UITraitCollection.verticalBarEdge, which is 0 (unspecified) where there's no vertical bar
            var selector = new Selector("verticalBarEdge");
            return traits.RespondsToSelector(selector) && traits.ValueForKey(new NSString("verticalBarEdge")) is NSNumber edge && edge.NIntValue != 0;
        }
    }
}
