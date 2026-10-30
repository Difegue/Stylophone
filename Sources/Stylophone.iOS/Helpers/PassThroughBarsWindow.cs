using System;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    /// <summary>
    /// Window letting the page's controls take the touches landing on them through the bars drawn above the page.
    /// Now Playing puts its playback controls in the bottom toolbar's row on phones in landscape, and iOS 26's toolbar
    /// takes every touch in that row, even between its items.
    /// </summary>
    public class PassThroughBarsWindow : UIWindow
    {
        public PassThroughBarsWindow(UIWindowScene windowScene) : base(windowScene)
        {
        }

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            var hitView = base.HitTest(point, uievent);
            if (uievent != null && uievent.Type != UIEventType.Touches)
                return hitView;

            // Touches on the bar's own controls stay there. iOS 26 draws its bars' items without any, so it goes by what's below.
            var bar = FindBar(hitView);
            if (bar?.Superview == null || IsInEnabledControl(hitView, bar))
                return hitView;

            // Views behind the bar, frontmost first
            foreach (var view in bar.Superview.Subviews.Reverse().SkipWhile(v => v != bar).Skip(1))
            {
                if (view.Hidden || !view.UserInteractionEnabled || view.Alpha < 0.01f)
                    continue;

                var underBar = view.HitTest(view.ConvertPointFromView(point, this), uievent);
                if (underBar != null)
                    return IsInEnabledControl(underBar, null) ? underBar : hitView;
            }

            return hitView;
        }

        /// <returns>The outermost bar holding the given view: a toolbar, or the container of iOS 26's floating bars.</returns>
        private static UIView FindBar(UIView view)
        {
            UIView bar = null;
            for (; view != null; view = view.Superview)
            {
                if (view is UIToolbar || view.Class.Name.Contains("FloatingBar"))
                    bar = view;
            }

            return bar;
        }

        private static bool IsInEnabledControl(UIView view, UIView container)
        {
            for (; view != null && view != container; view = view.Superview)
            {
                if (view is UIControl control)
                    return control.Enabled;
            }

            return false;
        }
    }
}
