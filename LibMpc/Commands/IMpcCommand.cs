using System.Collections.Generic;

namespace LibMpc
{
    public interface IMpcCommand<T>
    {
        string Value { get; }

        IReadOnlyDictionary<string, IList<T>> FormatResponse(IReadOnlyDictionary<string, IList<string>> response);
    }
}