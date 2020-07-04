// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdOutput.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Types
{
    /// <summary>
    /// Represents a MPD output.
    /// </summary>
    public class MpdOutput
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MpdOutput"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="plugin">The plugin name.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        public MpdOutput(int id, string name, string plugin, bool enabled)
        {
            this.Id = id;
            this.Name = name;
            this.Plugin = plugin;
            this.IsEnabled = enabled;
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the plugin name.
        /// </summary>
        /// <value>
        /// The plugin name.
        /// </value>
        public string Plugin { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; }
    }
}