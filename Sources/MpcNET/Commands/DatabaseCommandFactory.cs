// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabaseCommandFactory.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands
{
    using System.Collections.Generic;
    using MpcNET.Commands.Database;
    using MpcNET.Tags;
    using MpcNET.Types;

    /// <summary>
    /// https://www.musicpd.org/doc/protocol/database.html
    /// </summary>
    public class DatabaseCommandFactory : IDatabaseCommandFactory
    {
        /// <summary>
        /// Finds the specified tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="searchText">The search text.</param>
        /// <returns>An <see cref="FindCommand"/>.</returns>
        public IMpcCommand<IEnumerable<IMpdFile>> Find(ITag tag, string searchText)
        {
            return new FindCommand(tag, searchText);
        }

        /// <summary>
        /// Updates the specified URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>An <see cref="UpdateCommand"/>.</returns>
        public IMpcCommand<string> Update(string uri = null)
        {
            return new UpdateCommand(uri);
        }

        /// <summary>
        /// Lists all.
        /// </summary>
        /// <returns>A <see cref="ListAllCommand"/>.</returns>
        public IMpcCommand<IEnumerable<MpdDirectory>> ListAll()
        {
            return new ListAllCommand();
        }

        // TODO: count
        // TODO: rescan
    }
}