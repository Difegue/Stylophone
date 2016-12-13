using System.Text.RegularExpressions;

namespace LibMpc
{
    public interface IMpdResponseState
    {
        string Status { get; }
        string ErrorMessage { get; }
        string MpdError { get; }
        bool Error { get; }
        bool Connected { get; }
    }

    public class MpdResponseState : IMpdResponseState
    {
        private static readonly Regex ErrorPattern = new Regex("^ACK \\[(?<code>[0-9]*)@(?<nr>[0-9]*)] \\{(?<command>[a-z]*)} (?<message>.*)$");

        private readonly string _endLine;

        public MpdResponseState(string endLine, bool connected)
        {
            _endLine = endLine;
            Connected = connected;

            if (!string.IsNullOrEmpty(_endLine))
            {
                if (_endLine.Equals(Constants.Ok))
                {
                    Status = _endLine;
                    Error = false;
                }
                else
                {
                    ParseErrorResponse();
                }
            }
        }

        public bool Connected { get; } = false;
        public bool Error { get; } = true;
        public string Status { get; private set; } = "UNKNOWN";
        public string ErrorMessage { get; private set; } = string.Empty;
        public string MpdError { get; private set; } = string.Empty;

        private void ParseErrorResponse()
        {
            Status = "ERROR";
            MpdError = _endLine;

            var match = ErrorPattern.Match(_endLine);

            if (match.Groups.Count != 5)
            {
                ErrorMessage = "Unexpected response from server.";
            }
            else
            {
                var errorCode = match.Result("${code}");
                var commandListItem = match.Result("${nr}");
                var commandFailed = match.Result("${command}");
                var errorMessage = match.Result("${message}");
                ErrorMessage = $"ErrorCode: { errorCode }, CommandListItem: { commandListItem }, CommandFailed: { commandFailed }, ErrorMessage: { errorMessage }";
            }
        }
    }
}