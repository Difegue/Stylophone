using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace LibMpc
{
    public interface IMpdMessage<T>
    {
        IMpdRequest<T> Request { get; }
        IMpdResponse<T> Response { get; }
    }

    [DebuggerDisplay("Request: {Request.Command.Value} | Response Status: {Response.State.Status}")]
    public class MpdMessage<T> : IMpdMessage<T>
    {
        private readonly Regex _linePattern = new Regex("^(?<key>[A-Za-z_-]*):[ ]{0,1}(?<value>.*)$");
        private readonly IList<string> _rawResponse;

        public MpdMessage(IMpcCommand<T> command, bool connected, IReadOnlyCollection<string> response)
        {
            Request = new MpdRequest<T>(command);

            var endLine = response.Skip(response.Count - 1).Single();
            _rawResponse = response.Take(response.Count - 1).ToList();

            var values = Request.Command.FormatResponse(GetValuesFromResponse());
            Response = new MpdResponse<T>(endLine, values, connected);
        }

        public IMpdRequest<T> Request { get; }
        public IMpdResponse<T> Response { get; }

        private IList<KeyValuePair<string, string>> GetValuesFromResponse()
        {
            var result = new List<KeyValuePair<string, string>>();

            foreach (var line in _rawResponse)
            {
                var match = _linePattern.Match(line);
                if (match.Success)
                {
                    var mpdKey = match.Result("${key}");
                    if (!string.IsNullOrEmpty(mpdKey))
                    {
                        var mpdValue = match.Result("${value}");
                        if (!string.IsNullOrEmpty(mpdValue))
                        {
                            result.Add(new KeyValuePair<string, string>(mpdKey, mpdValue));
                        }
                    }
                }
            }

            return result;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}