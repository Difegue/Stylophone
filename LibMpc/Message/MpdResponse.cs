using System;
using System.Collections.Generic;

namespace LibMpc
{
    public interface IMpdResponse<T>
    {
        IMpdResponseState State { get; }
        IDictionary<string, T> Body { get; }
    }

    public class MpdResponse<T> : IMpdResponse<T>
    {
        public MpdResponse(string endLine, IDictionary<string, T> body, bool connected)
        {
            State = new MpdResponseState(endLine, connected);
            Body = body;
        }

        public IMpdResponseState State { get; }
        public IDictionary<string, T> Body { get; }
    }

    public static class CheckNotNullExtension
    {
        public static void CheckNotNull(this object toBeChecked)
        {
            if (toBeChecked == null)
            {
                throw new ArgumentNullException(nameof(toBeChecked));
            }
        }
    }
}
