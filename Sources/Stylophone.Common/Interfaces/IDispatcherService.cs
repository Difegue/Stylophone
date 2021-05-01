using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Stylophone.Common.Interfaces
{
    public interface IDispatcherService
    {
        Task ExecuteOnUIThreadAsync(Action p);
        Task<T> EnqueueAsync<T>(Func<Task<T>> function);
        void Initialize();
    }
}
