using System;
using Stylophone.Common.ViewModels;
using Stylophone.iOS.Helpers;

namespace Stylophone.iOS.ViewControllers
{
    public interface IViewController<T>: IPreparableViewController where T: ViewModelBase
    {
        T ViewModel { get; }

        PropertyBinder<T> Binder { get; }
    }

    public interface IPreparableViewController
    {
        void Prepare(object parameter)
        {

        }
    }
}
