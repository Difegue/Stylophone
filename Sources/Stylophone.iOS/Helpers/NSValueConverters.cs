using System;
using System.Runtime.CompilerServices;
using Foundation;
using ObjCRuntime;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using Stylophone.Common.ViewModels;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    [Register(nameof(StringToUIImageValueTransformer))]
    public class StringToUIImageValueTransformer : NSValueTransformer
    {
        public static new Class TransformedValueClass => new UIImage().Class;
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

    [Register(nameof(SkiaToUIImageValueTransformer))]
    public class SkiaToUIImageValueTransformer : NSValueTransformer
    {
        public static new Class TransformedValueClass => new UIImage().Class;
        public static new bool AllowsReverseTransformation => false;

        public override NSObject TransformedValue(NSObject value)
        {
            UIImage img;

            if (value is NSWrapper wrap)
            {
                var skiaImage = wrap.ManagedObject as SKImage;
                img = skiaImage.ToUIImage();
            }
            else
                img = UIImage.FromBundle("AlbumPlaceholder"); //Fallback

            return img;
        }

    }

    [Register(nameof(SkiaToUIColorValueTransformer))]
    public class SkiaToUIColorValueTransformer : NSValueTransformer
    {
        public static new Class TransformedValueClass => new UIColor(0,0).Class;
        public static new bool AllowsReverseTransformation => false;

        public override NSObject TransformedValue(NSObject value)
        {
            UIColor col;

            if (value is NSWrapper wrap)
            {
                var skiaColor = (SKColor)wrap.ManagedObject;
                col = skiaColor.ToUIColor();
            }
            else
                col = UIColor.SystemBlueColor;

            return col;
        }
    }

    [Register(nameof(IntToStringValueTransformer))]
    public class IntToStringValueTransformer : NSValueTransformer
    {
        public static new Class TransformedValueClass => new NSString().Class;
        public static new bool AllowsReverseTransformation => true;

        public override NSObject TransformedValue(NSObject value)
        {
            if (value is NSNumber num)
                return new NSString(num.Int64Value.ToString());

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
        public static new Class TransformedValueClass => new NSNumber(true).Class;
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

    [Register(nameof(NextTrackToStringValueTransformer))]
    public class NextTrackToStringValueTransformer : NSValueTransformer
    {
        public static new Class TransformedValueClass => new NSString().Class;
        public static new bool AllowsReverseTransformation => false;

        public override NSObject TransformedValue(NSObject value)
        {
            var text = "";

            if (value is NSWrapper wrap)
            {
                var trackVm = (TrackViewModel)wrap.ManagedObject;
                text = Localization.Strings.Resources.PlaybackUpNext + " " + trackVm.Name;
            }
                
            return new NSString(text);
        }

        public override NSObject ReverseTransformedValue(NSObject value)
        {
            int result = 0;

            if (value is NSString s)
                int.TryParse(s, out result);

            return NSNumber.FromInt32(result);
        }
    }
}
