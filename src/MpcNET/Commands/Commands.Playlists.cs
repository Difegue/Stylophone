using System.Collections.Generic;
using System.Linq;
using MpcNET.Types;

namespace MpcNET
{
    public static partial class Commands
    {
        public static class Playlists
        {
            /// <summary>
            /// https://www.musicpd.org/doc/protocol/queue.html
            /// </summary>
            public static class Current
            {
                /// <summary>
                /// Adds the file URI to the playlist (directories add recursively). URI can also be a single file.
                /// </summary>
                private class AddImpl : IMpcCommand<string>
                {
                    private readonly string _uri;

                    public AddImpl(string uri)
                    {
                        _uri = uri;
                    }

                    public string Value => string.Join(" ", "add", $"\"{_uri}\"");

                    public string FormatResponse(IList<KeyValuePair<string, string>> response)
                    {
                        return string.Join(", ", response);
                    }
                }

                /// <summary>
                /// Command: add
                /// </summary>
                public static IMpcCommand<string> AddDirectory(string directory) { return new AddImpl(directory); }

                /// <summary>
                /// Adds a song to the playlist (non-recursive) and returns the song id.
                /// </summary>
                private class AddIdImpl : IMpcCommand<string>
                {
                    private readonly string _uri;

                    public AddIdImpl(string uri)
                    {
                        _uri = uri;
                    }

                    public string Value => string.Join(" ", "addid", $"\"{_uri}\"");

                    public string FormatResponse(IList<KeyValuePair<string, string>> response)
                    {
                        return string.Join(", ", response);
                    }
                }

                /// <summary>
                /// Command: addid
                /// </summary>
                public static IMpcCommand<string> AddSong(string songPath) { return new AddIdImpl(songPath); }

                /// <summary>
                /// Clears the current playlist.
                /// </summary>
                private class ClearImpl : IMpcCommand<string>
                {
                    public string Value => "clear";

                    public string FormatResponse(IList<KeyValuePair<string, string>> response)
                    {
                        return string.Join(", ", response);
                    }
                }

                /// <summary>
                /// Command: clear
                /// </summary>
                public static IMpcCommand<string> Clear() { return new ClearImpl(); }

                /// <summary>
                /// Displays the current playlist.
                /// </summary>
                private class PlaylistImpl : IMpcCommand<IEnumerable<IMpdFile>>
                {
                    public string Value => "playlist";

                    public IEnumerable<IMpdFile> FormatResponse(IList<KeyValuePair<string, string>> response)
                    {
                        var results = response.Select(line => new MpdFile(line.Value) { Pos = int.Parse(line.Key) });

                        return results;
                    }
                }

                /// <summary>
                /// Command: playlist
                /// </summary>
                public static IMpcCommand<IEnumerable<IMpdFile>> GetAllSongsInfo() { return new PlaylistImpl(); }

                /// <summary>
                /// Deletes a song from the playlist.
                /// </summary>
                private class DeleteImpl : IMpcCommand<string>
                {
                    private readonly int _position;

                    public DeleteImpl(int position)
                    {
                        _position = position;
                    }

                    public string Value => string.Join(" ", "delete", _position);

                    public string FormatResponse(IList<KeyValuePair<string, string>> response)
                    {
                        return string.Join(", ", response);
                    }
                }

                /// <summary>
                /// Command: delete
                /// </summary>
                public static IMpcCommand<string> RemoveSongByPosition(int position) { return new DeleteImpl(position); }

                /// <summary>
                /// Deletes the song SONGID from the playlist
                /// </summary>
                private class DeleteIdImpl : IMpcCommand<string>
                {
                    private readonly int _songId;

                    public DeleteIdImpl(int songId)
                    {
                        _songId = songId;
                    }

                    public string Value => string.Join(" ", "deleteid", _songId);

                    public string FormatResponse(IList<KeyValuePair<string, string>> response)
                    {
                        return string.Join(", ", response);
                    }
                }

                /// <summary>
                /// Command: deleteid
                /// </summary>
                public static IMpcCommand<string> RemoveSongById(int songId) { return new DeleteIdImpl(songId); }

                /// <summary>
                /// Displays song ID in the playlist.
                /// </summary>
                private class PlaylistIdImpl : IMpcCommand<IEnumerable<IMpdFile>>
                {
                    private readonly int _songId;

                    public PlaylistIdImpl(int songId)
                    {
                        _songId = songId;
                    }

                    public string Value => string.Join(" ", "playlistid", _songId);

                    public IEnumerable<IMpdFile> FormatResponse(IList<KeyValuePair<string, string>> response)
                    {
                        var results = new List<MpdFile>();

                        foreach (var line in response)
                        {
                            if (line.Key.Equals("file"))
                            {
                                results.Add(new MpdFile(line.Value));
                            }
                            else
                            {
                                results.Last().AddTag(line.Key, line.Value);
                            }
                        }

                        return results;
                    }
                }

                /// <summary>
                /// Command: playlistid
                /// </summary>
                public static IMpcCommand<IEnumerable<IMpdFile>> GetSongMetadata(int songId) { return new PlaylistIdImpl(songId); }

                /// <summary>
                /// Displays a list of all songs in the playlist,
                /// </summary>
                private class PlaylistInfoImpl : IMpcCommand<IEnumerable<IMpdFile>>
                {
                    public string Value => "playlistinfo";

                    public IEnumerable<IMpdFile> FormatResponse(IList<KeyValuePair<string, string>> response)
                    {
                        var results = new List<MpdFile>();

                        foreach (var line in response)
                        {
                            if (line.Key.Equals("file"))
                            {
                                results.Add(new MpdFile(line.Value));
                            }
                            else
                            {
                                results.Last().AddTag(line.Key, line.Value);
                            }
                        }

                        return results;
                    }
                }

                /// <summary>
                /// Command: playlistinfo
                /// </summary>
                public static IMpcCommand<IEnumerable<IMpdFile>> GetAllSongMetadata() { return new PlaylistInfoImpl(); }
            }

            /// <summary>
            /// https://www.musicpd.org/doc/protocol/playlist_files.html
            /// </summary>
            public static class Stored
            {
                /// <summary>
                /// Loads the playlist into the current queue.
                /// </summary>
                private class LoadImpl : IMpcCommand<string>
                {
                    private readonly string _playlistName;

                    public LoadImpl(string playlistName)
                    {
                        _playlistName = playlistName;
                    }

                    public string Value => string.Join(" ", "load", $"\"{_playlistName}\"");

                    public string FormatResponse(IList<KeyValuePair<string, string>> response)
                    {
                        return string.Join(", ", response);
                    }
                }

                /// <summary>
                /// Command: load
                /// </summary>
                public static IMpcCommand<string> Load(string playlistName) { return new LoadImpl(playlistName); }

                /// <summary>
                /// Lists the songs in the playlist.
                /// </summary>
                private class ListPlaylistImpl : IMpcCommand<IEnumerable<IMpdFilePath>>
                {
                    private readonly string _playlistName;

                    public ListPlaylistImpl(string playlistName)
                    {
                        _playlistName = playlistName;
                    }

                    public string Value => string.Join(" ", "listplaylist", $"\"{_playlistName}\"");

                    public IEnumerable<IMpdFilePath> FormatResponse(IList<KeyValuePair<string, string>> response)
                    {
                        var results = response.Where(line => line.Key.Equals("file")).Select(line => new MpdFile(line.Value));

                        return results;
                    }
                }

                /// <summary>
                /// Command: listplaylist
                /// </summary>
                public static IMpcCommand<IEnumerable<IMpdFilePath>> GetContent(string playlistName) { return new ListPlaylistImpl(playlistName); }

                /// <summary>
                /// Lists the songs with metadata in the playlist.
                /// </summary>
                private class ListPlaylistInfoImpl : IMpcCommand<IEnumerable<IMpdFile>>
                {
                    private readonly string _playlistName;

                    public ListPlaylistInfoImpl(string playlistName)
                    {
                        _playlistName = playlistName;
                    }

                    public string Value => string.Join(" ", "listplaylistinfo", $"\"{_playlistName}\"");

                    public IEnumerable<IMpdFile> FormatResponse(IList<KeyValuePair<string, string>> response)
                    {
                        var results = new List<MpdFile>();

                        foreach (var line in response)
                        {
                            if (line.Key.Equals("file"))
                            {
                                results.Add(new MpdFile(line.Value));
                            }
                            else
                            {
                                results.Last().AddTag(line.Key, line.Value);
                            }
                        }

                        return results;
                    }
                }

                /// <summary>
                /// Command: listplaylistinfo
                /// </summary>
                public static IMpcCommand<IEnumerable<IMpdFile>> GetContentWithMetadata(string playlistName) { return new ListPlaylistInfoImpl(playlistName); }

                /// <summary>
                /// Prints a list of the playlist directory.
                /// </summary>
                private class ListPlaylistsImpl : IMpcCommand<IEnumerable<MpdPlaylist>>
                {
                    public string Value => "listplaylists";

                    public IEnumerable<MpdPlaylist> FormatResponse(IList<KeyValuePair<string, string>> response)
                    {
                        var result = new List<MpdPlaylist>();

                        foreach (var line in response)
                        {
                            if (line.Key.Equals("playlist"))
                            {
                                result.Add(new MpdPlaylist(line.Value));
                            }
                            else if (line.Key.Equals("Last-Modified"))
                            {
                                result.Last().AddLastModified(line.Value);
                            }
                        }

                        return result;
                    }
                }

                /// <summary>
                /// Command: listplaylists
                /// </summary>
                public static IMpcCommand<IEnumerable<MpdPlaylist>> GetAll() { return new ListPlaylistsImpl(); }
            }
        }
    }
}