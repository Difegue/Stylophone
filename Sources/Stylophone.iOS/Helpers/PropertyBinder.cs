using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Foundation;
using CommunityToolkit.Mvvm.ComponentModel;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    public class PropertyBinder<TObservable>: IDisposable
        where TObservable : ObservableObject
    {
        private readonly Dictionary<string, IList<Binding>> _bindings = new();
        private readonly Dictionary<string, IDisposable> _observers = new();
        private readonly Dictionary<UIButton, EventHandler> _buttonBindings = new();
        private TObservable _observableObject;

        public PropertyBinder(TObservable viewModel)
        {
            _observableObject = viewModel;
            _observableObject.PropertyChanged += OnObservablePropertyChanged;
        }
        
        public void Dispose()
        {
            if (_observableObject != null)
                _observableObject.PropertyChanged -= OnObservablePropertyChanged;

            // Dispose our observers
            foreach (IDisposable observer in _observers.Values)
                observer.Dispose();

            // Unregister our button bindings
            foreach (var kvp in _buttonBindings)
                kvp.Key.PrimaryActionTriggered -= kvp.Value;
        }

        // Shorthand method to attach a command to a UIButton.
        public void BindButton(UIButton button, string buttonText, ICommand command, object parameter = null)
        {
            button.SetTitle(buttonText, UIControlState.Normal);

            // Record button/eventhandler association so we can unregister them when the binder is disposed
            var evtHandler = new EventHandler((s, e) => command.Execute(parameter));
            button.PrimaryActionTriggered += evtHandler;

            _buttonBindings.Add(button, evtHandler);
        }

        public UIAction GetCommandAction(string actionText, string systemImage, ICommand command, object parameter = null)
        {
            return UIAction.Create(actionText, UIImage.GetSystemImage(systemImage), null,
                new UIActionHandler((sender) => command.Execute(parameter)));
        }

        public UIContextualAction GetContextualAction(UIContextualActionStyle style, string actionText, ICommand command, object parameter = null)
        {
            return UIContextualAction.FromContextualActionStyle(style, actionText,
                new UIContextualActionHandler((act, view, handler) =>
                {
                    if (command.CanExecute(parameter))
                    {
                        command.Execute(parameter);
                        handler.Invoke(true);
                    }
                    else
                    {
                        handler.Invoke(false);
                    }  
                })
            );
        }

        public void Bind<T>(NSObject obj, string keypath, string property, bool isTwoWay = false, NSValueTransformer valueTransformer = null)
            => Bind<T>(obj, new NSString(keypath), property, isTwoWay, valueTransformer);

        public void Bind<T>(NSObject obj, NSString keypath, string property, bool isTwoWay = false, NSValueTransformer valueTransformer = null)
        {
            var propertyInfo = GetProperty(property);
            var propertyValue = (T)propertyInfo.GetValue(_observableObject);

            var binding =
                new Binding { Object = obj, Keypath = keypath, Property = propertyInfo, ValueTransformer = valueTransformer };

            if (_bindings.ContainsKey(property))
                _bindings.GetValueOrDefault(property).Add(binding);
            else
                _bindings.Add(property, new List<Binding>(new[] { binding }));

            // Set the initial value
            binding.UpdateNSObject(propertyValue);

            if (isTwoWay)
            {
                // Create an Observer to keep track of UIKit-side changes
                // Note: This won't work if the NSObject isn't KVO-compliant. (which sadly happens often with UIKit...)
                var observer = obj.AddObserver(keypath, NSKeyValueObservingOptions.OldNew,
                    (c) => binding.UpdateProperty<T>(_observableObject, c));
                _observers.Add(obj.Handle.ToString() + "-" + keypath, observer);
            }
        }

        private void OnObservablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // The PropertyChanged event can indicate all properties on the object have changed by using either null or String.Empty as
            // the property name in the PropertyChangedEventArgs.
            if (string.IsNullOrEmpty(e.PropertyName))
            {
                foreach (var property in _bindings.Keys)
                {
                    var bindings = _bindings.GetValueOrDefault(property);
                    foreach (var binding in bindings)
                        binding.UpdateNSObject(binding.Property.GetValue(_observableObject));
                }
            }
            else if (_bindings.ContainsKey(e.PropertyName))
            {
                var bindings = _bindings.GetValueOrDefault(e.PropertyName);
                foreach (var binding in bindings)
                    binding.UpdateNSObject(binding.Property.GetValue(_observableObject));
            }
        }

        private PropertyInfo GetProperty(string propertyName)
        {
            if (_bindings.ContainsKey(propertyName))
                return _bindings[propertyName].First().Property;

            var propertyInfo = typeof(TObservable).GetProperty(propertyName);
            return propertyInfo ?? throw new ArgumentException($"Property {propertyName} not found on observable object {typeof(TObservable).Name}");
        }

    }
}