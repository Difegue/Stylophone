using System.Collections.Generic;
using System.Linq;

namespace LibMpc
{
    public interface IMpcCommand<T>
    {
        string Value { get; }

        IDictionary<string, T> FormatResponse(IList<KeyValuePair<string, string>> response);
    }

    internal static class MpdCommandExtensions
    {
        public static IDictionary<string, string> ToDefaultDictionary(this IList<KeyValuePair<string, string>> items)
        {
            return items.ToDictionary(item => item.Key, item => item.Value);
        }
    }
}