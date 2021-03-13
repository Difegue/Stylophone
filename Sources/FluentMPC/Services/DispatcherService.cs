using Microsoft.Toolkit.Uwp;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace FluentMPC.Services
{
    public static class DispatcherService
    {
        private static DispatcherQueue _queue;
        public static DispatcherQueue DispatcherQueue
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

        public static void Initialize()
        {
            // Get a DispatcherQueue instance for later use. This has to be called on the UI thread,
            // but it can then be cached for later use and accessed from a background thread as well.
            DispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        public static Task ExecuteOnUIThreadAsync(Action function, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal) => DispatcherQueue.EnqueueAsync(function, priority);
    }
}
