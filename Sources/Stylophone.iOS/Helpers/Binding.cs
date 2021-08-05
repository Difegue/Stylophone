using System;
using System.Diagnostics;
using System.Reflection;
using Foundation;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    public class NSWrapper : NSObject
    {
        public object ManagedObject { get; set; }
    }

    /// <summary>
    /// Model class for a lo-fi Cocoa Bindings reimplementation.
    /// You can apply a NSValueTransformer to convert the data as it goes between the NSObject and the ObservableObject.
    /// </summary>
    public class Binding
    {
        public NSObject Object { get; set; }
        public NSString Keypath { get; set; }
        public PropertyInfo Property { get; set; }
        public NSValueTransformer ValueTransformer { get; set; }

        /// <summary>
        /// Update the NSObject's keypath with the given value from the property.
        /// </summary>
        public void UpdateNSObject(object value)
        {
            // Try to box the value into a native type, and use a NSWrapper if we can't
            var nativeValue = NSObject.FromObject(value);
            if (nativeValue == null)
            {
                nativeValue = new NSWrapper { ManagedObject = value };
            }

            // Use the value transformer if it is set
            if (ValueTransformer != null)
                nativeValue = ValueTransformer.TransformedValue(nativeValue);

            UIApplication.SharedApplication.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (Object == null)
                        throw new ArgumentNullException("Backing NSObject is null!", new Exception());

                    if (!(nativeValue is NSNull))
                        Object.SetValueForKeyPath(nativeValue, Keypath);
                }
                catch (ObjectDisposedException)
                {
#if DEBUG
                    // The NSObject has been disposed -- This usually means the matching native object/view has gone away.
                    // Either keep a reference to the native view to prevent it from leaving, or destroy the PropertyBinder at the same time.
                    Debugger.Break();
#endif
                }
            });
        }

        public void UpdateProperty<T>(ObservableObject targetObservable, NSObservedChange change)
        {
            var nativeValue = change.NewValue;

            // Apply the ValueTransformer in reverse if it is set.
            if (ValueTransformer != null)
                nativeValue = ValueTransformer.ReverseTransformedValue(nativeValue);

            object value = typeof(T) switch
            {
                Type t when t == typeof(int) => ((NSNumber)nativeValue).Int32Value,
                Type t when t == typeof(long) => ((NSNumber)nativeValue).Int64Value,
                Type t when t == typeof(double) => ((NSNumber)nativeValue).DoubleValue,
                Type t when t == typeof(bool) => ((NSNumber)nativeValue).BoolValue,
                Type t when t == typeof(string) => ((NSString)nativeValue).ToString(),
                _ => ((NSWrapper)nativeValue).ManagedObject // This must be a NSWrapper
            };

            Property.SetValue(targetObservable, value);
        }
    };
}
