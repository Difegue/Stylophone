using System.Collections.Generic;

namespace MpcNET.Commands.Database
{
    // TODO: listallinfo
    // TODO: listfiles
    // TODO: lsinfo
    // TODO: readcomments
    // TODO: search
    // TODO: searchadd
    // TODO: searchaddpl

    public class UpdateCommand : IMpcCommand<string>
    {
        private readonly string uri;

        public UpdateCommand(string uri)
        {
            this.uri = uri;
        }

        // TODO: Extend command: < update [URI] >
        public string Value
        {
            get
            {
                if (string.IsNullOrEmpty(this.uri))
                {
                    return "update";
                }

                var newUri = this.uri;
                if (this.uri.StartsWith(@""""))
                {
                    newUri = @"""" + this.uri;
                }

                if (this.uri.EndsWith(@""""))
                {
                    newUri = this.uri + @"""";
                }

                return string.Join(" ", "update", newUri);
            }
        }

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }

    // TODO: rescan
}