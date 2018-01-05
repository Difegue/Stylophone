using System.Collections.Generic;
using MpcNET.Commands.Playlist;
using MpcNET.Types;

namespace MpcNET.Commands
{
    public static partial class Command
    {
        public static class Playlists
        {
            /// <summary>
            /// https://www.musicpd.org/doc/protocol/queue.html
            /// </summary>
            public static class Current
            {
                /// <summary>
                /// Command: add
                /// </summary>
                public static IMpcCommand<string> AddDirectory(string directory) { return new AddCommand(directory); }

                /// <summary>
                /// Command: addid
                /// </summary>
                public static IMpcCommand<string> AddSong(string songPath) { return new AddIdCommand(songPath); }

                /// <summary>
                /// Command: clear
                /// </summary>
                public static IMpcCommand<string> Clear() { return new ClearCommand(); }

                /// <summary>
                /// Command: playlist
                /// </summary>
                public static IMpcCommand<IEnumerable<IMpdFile>> GetAllSongsInfo() { return new PlaylistCommand(); }

                /// <summary>
                /// Command: delete
                /// </summary>
                public static IMpcCommand<string> RemoveSongByPosition(int position) { return new DeleteCommand(position); }

                /// <summary>
                /// Command: deleteid
                /// </summary>
                public static IMpcCommand<string> RemoveSongById(int songId) { return new DeleteIdCommand(songId); }

                /// <summary>
                /// Command: playlistid
                /// </summary>
                public static IMpcCommand<IEnumerable<IMpdFile>> GetSongMetadata(int songId) { return new PlaylistIdCommand(songId); }

                /// <summary>
                /// Command: playlistinfo
                /// </summary>
                public static IMpcCommand<IEnumerable<IMpdFile>> GetAllSongMetadata() { return new PlaylistInfoCommand(); }
            }

            /// <summary>
            /// https://www.musicpd.org/doc/protocol/playlist_files.html
            /// </summary>
            public static class Stored
            {

                /// <summary>
                /// Command: load
                /// </summary>
                public static IMpcCommand<string> Load(string playlistName) { return new LoadCommand(playlistName); }

                /// <summary>
                /// Command: listplaylist
                /// </summary>
                public static IMpcCommand<IEnumerable<IMpdFilePath>> GetContent(string playlistName) { return new ListPlaylistCommand(playlistName); }

                /// <summary>
                /// Command: listplaylistinfo
                /// </summary>
                public static IMpcCommand<IEnumerable<IMpdFile>> GetContentWithMetadata(string playlistName) { return new ListPlaylistInfoCommand(playlistName); }

                /// <summary>
                /// Command: listplaylists
                /// </summary>
                public static IMpcCommand<IEnumerable<MpdPlaylist>> GetAll() { return new ListPlaylistsCommand(); }
            }
        }
    }
}