// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputCommandFactory.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands
{
    using System.Collections.Generic;
    using MpcNET.Commands.Output;
    using MpcNET.Types;

    /// <summary>
    /// https://www.musicpd.org/doc/protocol/output_commands.html
    /// </summary>
    public class OutputCommandFactory : IOutputCommandFactory
    {
        /// <summary>
        /// Outputses this instance.
        /// </summary>
        /// <returns>An <see cref="OutputsCommand"/>.</returns>
        public IMpcCommand<IEnumerable<MpdOutput>> Outputs()
        {
            return new OutputsCommand();
        }

        /// <summary>
        /// Disables the output.
        /// </summary>
        /// <param name="outputId">The output identifier.</param>
        /// <returns>A <see cref="DisableOutputCommand"/>.</returns>
        public IMpcCommand<string> DisableOutput(int outputId)
        {
            return new DisableOutputCommand(outputId);
        }

        /// <summary>
        /// Enables the output.
        /// </summary>
        /// <param name="outputId">The output identifier.</param>
        /// <returns>A <see cref="EnableOutputCommand"/>.</returns>
        public IMpcCommand<string> EnableOutput(int outputId)
        {
            return new EnableOutputCommand(outputId);
        }

        /// <summary>
        /// Toggles the output.
        /// </summary>
        /// <param name="outputId">The output identifier.</param>
        /// <returns>A <see cref="ToggleOutputCommand"/>.</returns>
        public IMpcCommand<string> ToggleOutput(int outputId)
        {
            return new ToggleOutputCommand(outputId);
        }
    }
}