using System;
using System.Collections.Generic;

namespace LibMpc
{
    public interface IMpdResponse<T>
    {
        IMpdResponseState State { get; }
        IReadOnlyDictionary<string, IList<T>> Body { get; }
    }

    public class MpdResponse<T> : IMpdResponse<T>
    {
        public MpdResponse(string endLine, IReadOnlyDictionary<string, IList<T>> body, bool connected)
        {
            State = new MpdResponseState(endLine, connected);
            Body = body;
        }

        public IMpdResponseState State { get; }
        public IReadOnlyDictionary<string, IList<T>> Body { get; }
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
