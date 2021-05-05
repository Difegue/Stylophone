using Stylophone.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Stylophone.Mobile.Services
{
    public class DispatcherService : IDispatcherService
    {
        public Task<T> EnqueueAsync<T>(Func<Task<T>> function) => MainThread.InvokeOnMainThreadAsync<T>(function);

        public Task ExecuteOnUIThreadAsync(Action p) => MainThread.InvokeOnMainThreadAsync(p);

        public void Initialize()
        {
            //noop
        }
    }
}
