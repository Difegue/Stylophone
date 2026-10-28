using System;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    /// <summary>
    /// Visual effect view rendering Liquid Glass. Interface Builder can't set glass effects,
    /// so use this as the custom class of a Visual Effect View and configure it through "User Defined Runtime Attributes".
    /// Falls back to a blur on systems without Liquid Glass.
    /// </summary>
    [Register(nameof(GlassEffectView))]
    public class GlassEffectView : UIVisualEffectView
    {
        // Whether the glass reacts to touches like system glass controls. Turn this on for glass hosting buttons.
        [Export("glassInteractive")]
        public bool Interactive { get; set; }

        // Fully rounded shape (circle for square views)
        [Export("glassCapsule")]
        public bool Capsule { get; set; }

        // Corner radius used when the glass isn't a capsule
        [Export("glassCornerRadius")]
        public nfloat CornerRadius { get; set; }

        // More transparent glass, for elements resting on an already blurred background
        [Export("glassClear")]
        public bool Clear { get; set; }

        private UIColor _glassTintColor;

        public GlassEffectView(NativeHandle handle) : base(handle)
        {
        }

        void ReleaseDesignerOutlets()
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            ConfigureGlass();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            // Corner configurations take care of this on Liquid Glass
            if (!OperatingSystem.IsIOSVersionAtLeast(26))
                Layer.CornerRadius = Capsule ? nfloat.Min(Bounds.Width, Bounds.Height) / 2 : CornerRadius;
        }

        /// <summary>
        /// Change the tint of the glass with the system's glass transition.
        /// </summary>
        public void SetGlassTintColor(UIColor color, bool animated)
        {
            _glassTintColor = color;

            if (animated)
                Animate(0.3, UpdateEffect);
            else
                UpdateEffect();
        }

        private void ConfigureGlass()
        {
            if (OperatingSystem.IsIOSVersionAtLeast(26))
            {
                CornerConfiguration = Capsule ? UICornerConfiguration.CreateCapsule() :
                    UICornerConfiguration.CreateUniformCorners(UICornerRadius.CreateFixed(CornerRadius));
            }
            else
            {
                ClipsToBounds = true;
            }

            UpdateEffect();
        }

        private void UpdateEffect()
        {
            if (OperatingSystem.IsIOSVersionAtLeast(26))
            {
                var glass = UIGlassEffect.Create(Clear ? UIGlassEffectStyle.Clear : UIGlassEffectStyle.Regular);
                glass.Interactive = Interactive;
                glass.TintColor = _glassTintColor;
                Effect = glass;
            }
            else
            {
                Effect = UIBlurEffect.FromStyle(UIBlurEffectStyle.SystemThinMaterial);
                BackgroundColor = _glassTintColor?.ColorWithAlpha(0.5f);
            }
        }
    }

    /// <summary>
    /// Visual effect view rendering the glass views in its content view together, so they blend when close to each other.
    /// The UIKit equivalent of SwiftUI's GlassEffectContainer.
    /// </summary>
    [Register(nameof(GlassContainerEffectView))]
    public class GlassContainerEffectView : UIVisualEffectView
    {
        // Distance at which glass elements start blending together
        [Export("glassSpacing")]
        public nfloat Spacing { get; set; }

        public GlassContainerEffectView(NativeHandle handle) : base(handle)
        {
        }

        void ReleaseDesignerOutlets()
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            // Without Liquid Glass, this is only a transparent container for the blurs of its GlassEffectViews
            Effect = OperatingSystem.IsIOSVersionAtLeast(26) ? new UIGlassContainerEffect { Spacing = Spacing } : null;
        }

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            // Only intercept touches landing on the glass elements, not on the empty space around them
            var hitView = base.HitTest(point, uievent);
            return hitView == this || hitView == ContentView ? null : hitView;
        }
    }
}
