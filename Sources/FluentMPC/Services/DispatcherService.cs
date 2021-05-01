using Microsoft.Toolkit.Uwp;
using Stylophone.Common.Interfaces;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace FluentMPC.Services
{
    public class DispatcherService: IDispatcherService
    {
        private DispatcherQueue _queue;
        public DispatcherQueue DispatcherQueue
        {
            get
            {
                if (_queue == null)
                {
                    throw new Exception("The DispatcherQueue hasn't been cached yet!");
                }

                return _queue;
            }

            set
            {
                _queue = value;
            }
        }

        public void Initialize()
        {
            // Get a DispatcherQueue instance for later use. This has to be called on the UI thread,
            // but it can then be cached for later use and accessed from a background thread as well.
            DispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        public Task ExecuteOnUIThreadAsync(Action function) => DispatcherQueue.EnqueueAsync(function, DispatcherQueuePriority.Normal);
        public Task<T> EnqueueAsync<T>(Func<Task<T>> function) => DispatcherQueue.EnqueueAsync(function, DispatcherQueuePriority.Normal);
    }
}
