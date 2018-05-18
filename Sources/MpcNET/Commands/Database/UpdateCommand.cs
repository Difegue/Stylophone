// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Database
{
    using System.Collections.Generic;

    internal class UpdateCommand : IMpcCommand<string>
    {
        private readonly string uri;

        public UpdateCommand(string uri)
        {
            this.uri = uri;
        }

        public string Serialize()
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

        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}