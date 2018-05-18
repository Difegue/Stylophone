// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IOutputCommandFactory.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands
{
    using System.Collections.Generic;
    using MpcNET.Commands.Output;
    using MpcNET.Types;

    /// <summary>
    /// Provides output commands.
    /// </summary>
    public interface IOutputCommandFactory
    {
        /// <summary>
        /// Outputses this instance.
        /// </summary>
        /// <returns>An <see cref="OutputsCommand"/>.</returns>
        IMpcCommand<IEnumerable<MpdOutput>> Outputs();

        /// <summary>
        /// Disables the output.
        /// </summary>
        /// <param name="outputId">The output identifier.</param>
        /// <returns>A <see cref="DisableOutputCommand"/>.</returns>
        IMpcCommand<string> DisableOutput(int outputId);

        /// <summary>
        /// Enables the output.
        /// </summary>
        /// <param name="outputId">The output identifier.</param>
        /// <returns>A <see cref="EnableOutputCommand"/>.</returns>
        IMpcCommand<string> EnableOutput(int outputId);

        /// <summary>
        /// Toggles the output.
        /// </summary>
        /// <param name="outputId">The output identifier.</param>
        /// <returns>A <see cref="ToggleOutputCommand"/>.</returns>
        IMpcCommand<string> ToggleOutput(int outputId);
    }
}