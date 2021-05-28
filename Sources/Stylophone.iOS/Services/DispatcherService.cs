using Stylophone.Common.Interfaces;
using System;
using System.Threading.Tasks;
using UIKit;

namespace Stylophone.iOS.Services
{
    public class DispatcherService: IDispatcherService
    {

        public void Initialize()
        {
            // Not needed on iOS
        }

        public Task ExecuteOnUIThreadAsync(Action function)
        {
            var tcs = new TaskCompletionSource<bool>();

            UIApplication.SharedApplication.BeginInvokeOnMainThread(() =>
            {
                function.Invoke();
                tcs.SetResult(true);
            });

            return tcs.Task;
        }
        public Task<T> EnqueueAsync<T>(Func<Task<T>> function)
        {
            var tcs = new TaskCompletionSource<T>();

            UIApplication.SharedApplication.BeginInvokeOnMainThread(async () =>
            {
                var res = await function.Invoke();
                tcs.SetResult(res);
            });

            return tcs.Task;
        }
    }
}
