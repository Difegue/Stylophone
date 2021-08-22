using Microsoft.Toolkit.Mvvm.ComponentModel;
using Stylophone.Common.Interfaces;
using Stylophone.Localization.Strings;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Stylophone.Common.ViewModels
{
    /// <summary>
    /// Root class for all viewmodels.
    /// </summary>
    public abstract class ViewModelBase : ObservableObject
    {
        protected IDispatcherService _dispatcherService;

        /// <summary>
        /// Text to show in the header when displaying this ViewModel. Don't fill it if you don't want a header.
        /// </summary>
        public static string GetHeader() => "";

        public ViewModelBase(IDispatcherService dispatcherService)
        {
            _dispatcherService = dispatcherService;
        }

        /// <summary>
        /// Compares the current and new values for a given property.If the value has changed,
        /// raises the Microsoft.Toolkit.Mvvm.ComponentModel.ObservableObject.PropertyChanging
        /// event, updates the property with the new value, then raises the Microsoft.Toolkit.Mvvm.ComponentModel.ObservableObject.PropertyChanged
        /// event.
        /// 
        /// This wrapper method calls SetProperty on the UI Thread.
        /// </summary>
        /// <returns></returns>
        protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            var oldValue = field;
            field = newValue;

            _dispatcherService.ExecuteOnUIThreadAsync(() => SetProperty(ref oldValue, newValue, propertyName));

            if (newValue != null)
                return !newValue.Equals(oldValue);
            else
                return false;
        }
    }
}
