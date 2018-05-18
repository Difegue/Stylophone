// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionCommandFactory.cs" company="MpcNET">
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
    /// https://www.musicpd.org/doc/protocol/reflection_commands.html.
    /// </summary>
    public class ReflectionCommandFactory : IReflectionCommandFactory
    {
        /// <summary>
        /// Gets a commands command.
        /// </summary>
        /// <returns>A <see cref="CommandsCommand"/>.</returns>
        public IMpcCommand<IEnumerable<string>> Commands()
        {
            return new CommandsCommand();
        }

        /// <summary>
        /// Gets a tag types command.
        /// </summary>
        /// <returns>A <see cref="TagTypesCommand"/>.</returns>
        public IMpcCommand<IEnumerable<string>> TagTypes()
        {
            return new TagTypesCommand();
        }

        /// <summary>
        /// Gets URL handlers command.
        /// </summary>
        /// <returns>A <see cref="UrlHandlersCommand"/>.</returns>
        public IMpcCommand<IEnumerable<string>> UrlHandlers()
        {
            return new UrlHandlersCommand();
        }

        /// <summary>
        /// Gets a decoders command.
        /// </summary>
        /// <returns>A <see cref="DecodersCommand"/>.</returns>
        public IMpcCommand<IEnumerable<MpdDecoderPlugin>> Decoders()
        {
            return new DecodersCommand();
        }
    }
}
