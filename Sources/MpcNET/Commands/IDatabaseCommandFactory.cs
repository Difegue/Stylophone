// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDatabaseCommandFactory.cs" company="Hukano">
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
    /// Provides database commands.
    /// </summary>
    public interface IDatabaseCommandFactory
    {
        /// <summary>
        /// Finds the specified tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="searchText">The search text.</param>
        /// <returns>An <see cref="FindCommand"/>.</returns>
        IMpcCommand<IEnumerable<IMpdFile>> Find(ITag tag, string searchText);

        /// <summary>
        /// Updates the specified URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>An <see cref="UpdateCommand"/>.</returns>
        IMpcCommand<string> Update(string uri = null);

        /// <summary>
        /// Lists all.
        /// </summary>
        /// <returns>A <see cref="ListAllCommand"/>.</returns>
        IMpcCommand<IEnumerable<MpdDirectory>> ListAll();
    }
}