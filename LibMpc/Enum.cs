namespace LibMpc
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