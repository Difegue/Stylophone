using System;
using Foundation;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    [Register(nameof(KvoUISwitch))]
    public class KvoUISwitch : UISwitch
    {

        public KvoUISwitch(IntPtr handle) : base(handle)
        {
        }

        void ReleaseDesignerOutlets()
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            AddTarget(NotifyChange, UIControlEvent.ValueChanged);
        }

        private void NotifyChange(object sender, EventArgs e)
        {
            // low-budget kvo compliance
            WillChangeValue("on");
            DidChangeValue("on");
        }
    }

    [Register(nameof(KvoUISlider))]
    public class KvoUISlider : UISlider
    {

        public KvoUISlider(IntPtr handle) : base(handle)
        {
        }

        void ReleaseDesignerOutlets()
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            AddTarget(NotifyChange, UIControlEvent.ValueChanged);
        }

        private void NotifyChange(object sender, EventArgs e)
        {
            // low-budget kvo compliance
            WillChangeValue("value");
            DidChangeValue("value");
        }
    }
}
