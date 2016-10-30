/*
 * Copyright 2008 Matthias Sessler
 * 
 * This file is part of LibMpc.net.
 *
 * LibMpc.net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 2.1 of the License, or
 * (at your option) any later version.
 *
 * LibMpc.net is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with LibMpc.net.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Libmpc
{
    /// <summary>
    /// The MpdOutput class contains all attributes of an output device of the MPD.
    /// </summary>
    public class MpdOutput
    {
        private readonly int id;
        private readonly string name;
        private readonly bool enabled;
        /// <summary>
        /// The id of the output.
        /// </summary>
        public int Id { get { return this.id; } }
        /// <summary>
        /// The name of the output.
        /// </summary>
        public string Name { get { return this.name; } }
        /// <summary>
        /// If the output is enabled.
        /// </summary>
        public bool IsEnabled { get { return this.enabled; } }
        /// <summary>
        /// Creates a new MpdOutput object.
        /// </summary>
        /// <param name="id">The id of the output.</param>
        /// <param name="name">The name of the output.</param>
        /// <param name="enabled">If the output is enabled.</param>
        public MpdOutput(int id, string name, bool enabled)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.id = id;
            this.name = name;
            this.enabled = enabled;
        }
        /// <summary>
        /// Returns a string representation of the object mainly for debuging purpose.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString()
        {
            return this.id + "::" + this.name + "::" + this.enabled;
        }
    }
}
