using System;
using Stylophone.Common.ViewModels;
using Stylophone.iOS.Helpers;

namespace Stylophone.iOS.ViewControllers
{
    public interface IViewController<T> where T: ViewModelBase
    {
        T ViewModel { get; }

        PropertyBinder<T> Binder { get; }

        void Prepare(object parameter)
        {

        }
    }
}
