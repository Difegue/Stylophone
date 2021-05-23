using System;
using System.Runtime.CompilerServices;
using Foundation;
using ObjCRuntime;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    [Register(nameof(StringToUIImageValueTransformer))]
    public class StringToUIImageValueTransformer : NSValueTransformer
    {
        public static new ObjCRuntime.Class TransformedValueClass => new UIImage().Class;
        public static new bool AllowsReverseTransformation => false;

        public override NSObject TransformedValue(NSObject value)
        {
            UIImage img;

            if (value is NSString str)
                img = UIImage.GetSystemImage(str);
            else
                img = UIImage.GetSystemImage("opticaldisc"); //Fallback

            return img;
        }

    }

    [Register(nameof(IntToStringValueTransformer))]
    public class IntToStringValueTransformer : NSValueTransformer
    {
        public static new ObjCRuntime.Class TransformedValueClass => new NSString().Class;
        public static new bool AllowsReverseTransformation => true;

        public override NSObject TransformedValue(NSObject value)
        {
            if (value is NSNumber num)
                return new NSString(value.ToString());

            return null;
        }

        public override NSObject ReverseTransformedValue(NSObject value)
        {
            int result = 0;

            if (value is NSString s)
                int.TryParse(s, out result);

            return NSNumber.FromInt32(result);
        }
    }

    [Register(nameof(ReverseBoolValueTransformer))]
    public class ReverseBoolValueTransformer : NSValueTransformer
    {
        public static new ObjCRuntime.Class TransformedValueClass => new NSNumber(true).Class;
        public static new bool AllowsReverseTransformation => true;

        public override NSObject TransformedValue(NSObject value)
        {
            var boolValue = false;

            if (value is NSNumber num)
                boolValue = !num.BoolValue;

            return new NSNumber(boolValue);
        }

        public override NSObject ReverseTransformedValue(NSObject value) => TransformedValue(value);
    }
}
