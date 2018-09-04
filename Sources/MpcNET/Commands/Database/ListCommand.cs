// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Database
{
    using System.Collections.Generic;
    using MpcNET.Tags;

    /// <summary>
    /// Lists the specified tag.
    /// https://www.musicpd.org/doc/protocol/database.html.
    /// </summary>
    public class ListCommand : IMpcCommand<string>
    {
        private readonly ITag tag;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListCommand"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        public ListCommand(ITag tag)
        {
            this.tag = tag;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => string.Join(" ", "list", this.tag);

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            // TODO:
            return response.ToString();
        }
    }

    // TODO: rescan
}