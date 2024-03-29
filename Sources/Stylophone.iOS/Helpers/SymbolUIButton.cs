// This file has been autogenerated from a class added in the UI designer.

using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Stylophone.iOS.Views
{
	public partial class SymbolUIButton : UIButton
	{
		public SymbolUIButton (IntPtr handle) : base (handle)
		{
			
		}

		public override void AwakeFromNib()
        {
            // https://stackoverflow.com/questions/58338420/sf-symbol-looks-distorted
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            // https://www.roryba.in/programming/swift/2018/03/24/animating-uibutton.html
            var identity = CGAffineTransform.MakeIdentity();
            var scaled = CGAffineTransform.MakeIdentity();
            scaled.Scale((nfloat)0.95, (nfloat)0.95, MatrixOrder.Prepend);

            AddTarget((s,e) => Animate(scaled), UIControlEvent.TouchDown | UIControlEvent.TouchDragEnter);
            AddTarget((s, e) => Animate(identity), UIControlEvent.TouchDragExit | UIControlEvent.TouchCancel | UIControlEvent.TouchUpInside | UIControlEvent.TouchUpOutside);

        }

        private void Animate(CGAffineTransform transform) 
        {
            Animate(0.4, 0, UIViewAnimationOptions.CurveEaseInOut,
                () => { Transform = transform; },
                null);
        }

       
    }
}
