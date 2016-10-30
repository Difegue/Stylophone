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
namespace Libmpc
{
    /// <summary>
    /// The scope specifier for search commands.
    /// </summary>
    public enum ScopeSpecifier
    {
        /// <summary>
        /// Any attribute of a file.
        /// </summary>
        Any,
        /// <summary>
        /// The path and name of the file.
        /// </summary>
        Filename,
        /// <summary>
        /// The artist of the track.
        /// </summary>
        Artist,
        /// <summary>
        /// The album the track appears on.
        /// </summary>
        Album,
        /// <summary>
        /// The title of the track.
        /// </summary>
        Title,
        /// <summary>
        /// The index of the track on its album.
        /// </summary>
        Track,
        /// <summary>
        /// The name of the track.
        /// </summary>
        Name,
        /// <summary>
        /// The genre of the song.
        /// </summary>
        Genre,
        /// <summary>
        /// The date the track was released.
        /// </summary>
        Date,
        /// <summary>
        /// The composer of the song.
        /// </summary>
        Composer,
        /// <summary>
        /// The performer of the song.
        /// </summary>
        Performer,
        /// <summary>
        /// A comment for the track.
        /// </summary>
        Comment,
        /// <summary>
        /// The disc of a multidisc album the track is on.
        /// </summary>
        Disc
    }

}