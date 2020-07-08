// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Database
{
    using System.Collections.Generic;

    /// <summary>
    /// Updates the specified URI.
    /// https://www.musicpd.org/doc/protocol/database.html.
    /// </summary>
    public class UpdateCommand : IMpcCommand<string>
    {
        private readonly string uri;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCommand"/> class.
        /// </summary>
        public UpdateCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCommand"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        public UpdateCommand(string uri)
        {
            this.uri = uri;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
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

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public string Deserialize(SerializedResponse response)
        {
            return string.Join(", ", response.ResponseValues);
        }
    }
}