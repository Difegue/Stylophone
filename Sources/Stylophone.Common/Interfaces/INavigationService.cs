using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace Stylophone.Common.Interfaces
{
    public class CoreNavigationEventArgs : EventArgs { public Type NavigationTarget { get; set; } public object Parameter { get; set; } }

    public interface INavigationService
    {
        Type CurrentPageViewModelType { get; }
        bool CanGoBack { get; }
        bool GoBack();

        void Navigate(Type viewmodel, object parameter = null);
        void Navigate<T>(object parameter = null) where T : ObservableObject;
        void SetListDataItemForNextConnectedAnimation(object item);

    }

    public abstract class NavigationServiceBase: INavigationService
    {
        public event EventHandler<CoreNavigationEventArgs> Navigated;

        public void Navigate<T>(object parameter = null) where T : ObservableObject => Navigate(typeof(T), parameter);
        public void Navigate(Type viewmodel, object parameter = null)
        {
            NavigateImplementation(viewmodel, parameter);
            Navigated?.Invoke(this, new CoreNavigationEventArgs { NavigationTarget = viewmodel, Parameter = parameter});
        }
        public abstract void NavigateImplementation(Type viewmodel, object parameter = null);

        public bool GoBack()
        {
            var result = GoBackImplementation();

            if (result)
            {
                // Get the viewmodel we landed back on from the implementation, and send an event with it
                Navigated?.Invoke(this, new CoreNavigationEventArgs { NavigationTarget = CurrentPageViewModelType, Parameter = null });
            }
            return result;
        }
        public abstract bool GoBackImplementation();

        public abstract Type CurrentPageViewModelType { get; }
        public abstract bool CanGoBack { get; }
        public abstract void SetListDataItemForNextConnectedAnimation(object item);
    }
}
