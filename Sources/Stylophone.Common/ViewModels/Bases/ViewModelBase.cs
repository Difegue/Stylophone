using CommunityToolkit.Mvvm.ComponentModel;
using Stylophone.Common.Interfaces;
using Stylophone.Localization.Strings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// This wrapper method calls OnPropertyChanged on the UI Thread. Thanks WinRT!
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            _dispatcherService.ExecuteOnUIThreadAsync(() => base.OnPropertyChanged(e));
        }
    }
}
