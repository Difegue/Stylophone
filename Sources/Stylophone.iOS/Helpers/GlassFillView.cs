using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    /// <summary>
    /// Color filling a Liquid Glass shape from behind the glass, which refracts it like the content underneath.
    /// Tinted glass alone only takes on a wash of its color: this is how the playback bubble gets its vivid fill.
    /// </summary>
    public class GlassFillView : UIView
    {
        /// <summary>
        /// Opacity of fill colors, slightly translucent so they still read as liquid inside the glass.
        /// </summary>
        public const float FillAlpha = 0.85f;

        private readonly UIView _glass;

        // Follows the glass' press animation
        private CADisplayLink _pressFollower;
        private bool _isPressed;
        private int _restingFrames;

        private GlassFillView(UIView glass)
        {
            _glass = glass;
            UserInteractionEnabled = false;
            ClipsToBounds = true;
            TranslatesAutoresizingMaskIntoConstraints = false;
        }

        /// <summary>
        /// Add a fill behind the given glass shape, with the same place and size. It goes behind the glass container the shape is in,
        /// so all the glass shapes around it refract it: drawn inside the shape, it'd be covered by the shapes merging with it
        /// whenever they're pressed and lift over it.
        /// </summary>
        public static GlassFillView InsertBehind(UIView glass)
        {
            UIView behind = glass;
            for (var view = glass.Superview; view != null; view = view.Superview)
            {
                if (view is GlassContainerEffectView)
                {
                    behind = view;
                    break;
                }
            }

            var fill = new GlassFillView(glass);
            behind.Superview.InsertSubviewBelow(fill, behind);
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                fill.CenterXAnchor.ConstraintEqualTo(glass.CenterXAnchor),
                fill.CenterYAnchor.ConstraintEqualTo(glass.CenterYAnchor),
                fill.WidthAnchor.ConstraintEqualTo(glass.WidthAnchor),
                fill.HeightAnchor.ConstraintEqualTo(glass.HeightAnchor)
            });

            return fill;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            // Fills are for capsule shaped glass
            Layer.CornerRadius = nfloat.Min(Bounds.Width, Bounds.Height) / 2;
        }

        public void FollowPresses(UIControl control)
        {
            control.TouchDown += (s, e) => StartFollowingPress();
            control.TouchUpInside += (s, e) => _isPressed = false;
            control.TouchUpOutside += (s, e) => _isPressed = false;
            control.TouchCancel += (s, e) => _isPressed = false;
        }

        /// <summary>
        /// Pressed glass lifts and stretches, then springs back. UIKit animates that through a layer it wraps the glass in, 
        /// which our fill behind the glass isn't part of. We read the glass' animated layer and apply the same transform to our fill to keep it in sync. 
        /// </summary>
        private void StartFollowingPress()
        {
            _isPressed = true;
            if (_pressFollower != null)
                return;

            _restingFrames = 0;
            _pressFollower = CADisplayLink.Create(FollowPress);
            _pressFollower.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
        }

        private void FollowPress()
        {
            // Get the current position of the glass as it's being stretched by the press
            var drawnGlass = _glass.Layer.PresentationLayer;
            var fillSuperlayer = Superview?.Layer.PresentationLayer;
            var restSize = Bounds.Size;
            if (drawnGlass == null || fillSuperlayer == null || restSize.Width == 0 || restSize.Height == 0)
                return;

            // Create a transform that makes the fill match the glass' drawn stretch, so the fill is always behind the glass. 
            // Bounds and center aren't affected by the transform we give the fill.
            var drawn = drawnGlass.ConvertRectToLayer(_glass.Bounds, fillSuperlayer);
            var scaleX = drawn.Width / restSize.Width;
            var scaleY = drawn.Height / restSize.Height;
            var offsetX = drawn.GetMidX() - Center.X;
            var offsetY = drawn.GetMidY() - Center.Y;

            CATransaction.Begin();
            CATransaction.DisableActions = true;
            Layer.Transform = CATransform3D.MakeFromAffine(new CGAffineTransform(scaleX, 0, 0, scaleY, offsetX, offsetY));
            CATransaction.Commit();

            // Stop once the glass is released and has settled back
            var isAtRest = nfloat.Abs(scaleX - 1) < 0.001 && nfloat.Abs(scaleY - 1) < 0.001 &&
                           nfloat.Abs(offsetX) < 0.05 && nfloat.Abs(offsetY) < 0.05;
            _restingFrames = isAtRest && !_isPressed ? _restingFrames + 1 : 0;

            if (_restingFrames > 10)
            {
                _pressFollower.Invalidate();
                _pressFollower = null;
                Layer.Transform = CATransform3D.Identity;
            }
        }
    }
}
