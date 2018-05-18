// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IReflectionCommandFactory.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands
{
    using System.Collections.Generic;
    using MpcNET.Commands.Reflection;
    using MpcNET.Types;

    /// <summary>
    /// Provides reflection commands.
    /// </summary>
    public interface IReflectionCommandFactory
    {
        /// <summary>
        /// Gets a commands command.
        /// </summary>
        /// <returns>A <see cref="CommandsCommand"/>.</returns>
        IMpcCommand<IEnumerable<string>> Commands();

        /// <summary>
        /// Gets a tag types command.
        /// </summary>
        /// <returns>A <see cref="TagTypesCommand"/>.</returns>
        IMpcCommand<IEnumerable<string>> TagTypes();

        /// <summary>
        /// Gets URL handlers command.
        /// </summary>
        /// <returns>A <see cref="UrlHandlersCommand"/>.</returns>
        IMpcCommand<IEnumerable<string>> UrlHandlers();

        /// <summary>
        /// Gets a decoders command.
        /// </summary>
        /// <returns>A <see cref="DecodersCommand"/>.</returns>
        IMpcCommand<IEnumerable<MpdDecoderPlugin>> Decoders();
    }
}