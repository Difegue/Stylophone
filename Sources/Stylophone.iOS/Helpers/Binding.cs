using System;
using System.Diagnostics;
using System.Reflection;
using Foundation;
using CommunityToolkit.Mvvm.ComponentModel;
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

        // Fade views over to their new values instead of switching them at once
        public bool CrossDissolve { get; set; }

        // Set while the NSObject is being updated from the property, so two-way bindings don't send that change back (see UpdateProperty)
        private bool _isUpdatingNSObject;

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
                    {
                        _isUpdatingNSObject = true;

                        if (CrossDissolve && Object is UIView view)
                            UIView.Transition(view, 0.3, UIViewAnimationOptions.TransitionCrossDissolve | UIViewAnimationOptions.AllowUserInteraction,
                                () => view.SetValueForKeyPath(nativeValue, Keypath), null);
                        else
                            Object.SetValueForKeyPath(nativeValue, Keypath);
                    }
                }
                catch (ObjectDisposedException)
                {
#if DEBUG
                    // The NSObject has been disposed -- This usually means the matching native object/view has gone away.
                    // Either keep a reference to the native view to prevent it from leaving, or destroy the PropertyBinder at the same time.
                    Debugger.Break();
#endif
                }
                finally
                {
                    _isUpdatingNSObject = false;
                }
            });
        }

        public void UpdateProperty<T>(ObservableObject targetObservable, NSObservedChange change)
        {
            // Ignore a property change coming from ourselves in the case of 2way bindings
            if (_isUpdatingNSObject)
                return;

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
