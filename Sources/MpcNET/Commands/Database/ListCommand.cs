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

    internal class ListCommand : IMpcCommand<string>
    {
        private readonly ITag tag;

        public ListCommand(ITag tag)
        {
            this.tag = tag;
        }

        public string Serialize() => string.Join(" ", "list", this.tag);

        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            // TODO:
            return response.ToString();
        }
    }

    // TODO: rescan
}