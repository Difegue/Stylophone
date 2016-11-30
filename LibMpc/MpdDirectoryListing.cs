using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace LibMpc
{
    /// <summary>
    /// The MpdDirectoryListing class contains the response of a MPD server to a list command.
    /// </summary>
    public class MpdDirectoryListing
    {
        private readonly ReadOnlyCollection<MpdFile> file;
        private readonly ReadOnlyCollection<string> directory;
        private readonly ReadOnlyCollection<string> playlist;
        /// <summary>
        /// The list of files in the directory.
        /// </summary>
        public ReadOnlyCollection<MpdFile> FileList { get { return this.file; } }
        /// <summary>
        /// The list of subdirectories in the directory.
        /// </summary>
        public ReadOnlyCollection<string> DirectoryList { get { return this.directory; } }
        /// <summary>
        /// The list of playlists in the directory.
        /// </summary>
        public ReadOnlyCollection<string> PlaylistList { get { return this.playlist; } }
        /// <summary>
        /// Creates a new MpdDirectoryListing.
        /// </summary>
        /// <param name="file">The list of files in the directory.</param>
        /// <param name="directory">The list of subdirectories in the directory.</param>
        /// <param name="playlist">The list of playlists in the directory.</param>
        public MpdDirectoryListing(List<MpdFile> file, List<string> directory, List<string> playlist)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (playlist == null)
                throw new ArgumentNullException("playlist");

            this.file = new ReadOnlyCollection<MpdFile>(file);
            this.directory = new ReadOnlyCollection<string>(directory);
            this.playlist = new ReadOnlyCollection<string>(playlist);
        }
        /// <summary>
        /// Creates a new MpdDirectoryListing.
        /// </summary>
        /// <param name="file">The list of files in the directory.</param>
        /// <param name="directory">The list of subdirectories in the directory.</param>
        /// <param name="playlist">The list of playlists in the directory.</param>
        public MpdDirectoryListing(ReadOnlyCollection<MpdFile> file, ReadOnlyCollection<string> directory, ReadOnlyCollection<string> playlist)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (playlist == null)
                throw new ArgumentNullException("playlist");

            this.file = file;
            this.directory = directory;
            this.playlist = playlist;
        }
    }
}
