using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibMpc
{
    public interface IMpdResponse
    {
        IEnumerable<string> Response { get; }
        IReadOnlyDictionary<string, string> Values { get; }
        IMpdResponseState State { get; }
    }

    public class MpdResponse : IMpdResponse
    {
        private static readonly Regex LinePattern = new Regex("^(?<key>[A-Za-z]*):[ ]{0,1}(?<value>.*)$");

        public MpdResponse(ICollection<string> response)
        {
            response.CheckNotNull();

            var endLine = response.Take(response.Count - 2).Single();

            Response = response.Take(response.Count - 2).ToList();
            State = new MpdResponseState(endLine);
            Values = GetValuesFromResponse();
        }

        public IMpdResponseState State { get; }

        public IEnumerable<string> Response { get; }

        public IReadOnlyDictionary<string, string> Values { get; }


        private IReadOnlyDictionary<string, string> GetValuesFromResponse()
        {
            var result = new Dictionary<string, string>();

            foreach (var line in Response)
            {
                var match = LinePattern.Match(line);
                if (match.Success)
                {
                    var mpdKey = match.Result("${key}");
                    if (!string.IsNullOrEmpty(mpdKey))
                    {
                        var mpdValue = match.Result("${value}");
                        if (string.IsNullOrEmpty(mpdValue))
                        {
                            result.Add(mpdKey, mpdValue);

                        }
                    }
                }
            }

            return result;
        }
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
