using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace LibMpc
{
    public interface IMpc
    {
        bool IsConnected { get; }
        bool Connect();

        event EventHandler Connected;
        event EventHandler Disconnected;

        MpdOutput[] Outputs();
        MpdStatistics Stats();
    }

    /// <summary>
    /// The Mpc class implements all commands for the MPD. It takes care of command building
    /// and parsing the response into .net objects.
    /// </summary>
    public class Mpc : IMpc
    {
        private static readonly Regex STATUS_AUDIO_REGEX = new Regex("^(?<sampleRate>[0-9]*):(?<bits>[0-9]*):(?<channels>[0-9]*)$");

        private MpcConnection _connection;
        private IPEndPoint _server;

        public Mpc(IPEndPoint server)
        {
            _server = server;
        }

        /// <summary>
        /// Is fired when a connection to a MPD server is established.
        /// </summary>
        public event EventHandler Connected
        {
            add { ConnectedRelayEvent += value; }
            remove { ConnectedRelayEvent -= value; }
        }

        private event EventHandler ConnectedRelayEvent;

        /// <summary>
        /// Is fired when the connection to the MPD server is closed.
        /// </summary>
        public event EventHandler Disconnected
        {
            add { DisconnectedRelayEvent += value; }
            remove { DisconnectedRelayEvent -= value; }
        }

        private event EventHandler DisconnectedRelayEvent;

        /// <summary>
        /// Connection status to MPD Server.
        /// </summary>
        public bool IsConnected => _connection?.IsConnected ?? false;

        public bool Connect()
        {
            if (_connection == null)
            {
                _connection = new MpcConnection(_server);
                _connection.Connected += OnConnected;
                _connection.Disconnected += OnDisconnected;
            }

            if (!_connection.IsConnected)
            {
                _connection.Connect();
            }

            return _connection.IsConnected;
        }

        private void OnConnected(object sender, EventArgs e)
        {
            ConnectedRelayEvent?.Invoke(this, e);
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            DisconnectedRelayEvent?.Invoke(this, e);
        }

        #region Admin Commands
        /// <summary>
        /// Disables an MPD output.
        /// </summary>
        /// <param name="id">The id of the output.</param>
        /// <returns>If the action was successful.</returns>
        public bool DisableOutput(int id)
        {
            return !_connection.Exec("disableoutput", new string[] { id.ToString() }).IsError;
        }
        /// <summary>
        /// Enables an MPD output.
        /// </summary>
        /// <param name="id">The id of the output.</param>
        /// <returns>If the action was successful.</returns>
        public bool EnableOutput(int id)
        {
            return !_connection.Exec("enableoutput", new string[] { id.ToString() }).IsError;
        }
        /// <summary>
        /// Lists all outputs of the MPD.
        /// </summary>
        /// <returns>The list of all MPD outputs.</returns>
        public MpdOutput[] Outputs()
        {
            MpdResponse response = _connection.Exec("outputs");
            if (response.Message.Count % 3 != 0)
                throw new InvalidMpdResponseException();

            MpdOutput[] ret = new MpdOutput[response.Message.Count / 3];

            for (int i = 0; i < ret.Length; i++)
            {
                int id;
                string name;
                int enabled;

                KeyValuePair<string, string> idLine = response[i * 3];
                if (idLine.Key == null)
                    throw new InvalidMpdResponseException("Invalid form of line " + (i * 3));
                if (!idLine.Key.Equals("outputid"))
                    throw new InvalidMpdResponseException("Key of line " + (i * 3) + " is not 'outputid'");
                if (!int.TryParse(idLine.Value, out id))
                    throw new InvalidMpdResponseException("Value of line " + (i * 3) + " is not a number");

                KeyValuePair<string, string> nameLine = response[i * 3 + 1];
                if (nameLine.Key == null)
                    throw new InvalidMpdResponseException("Invalid form of line " + (i * 3 + 1));
                if (!nameLine.Key.Equals("outputname"))
                    throw new InvalidMpdResponseException("Key of line " + (i * 3 + 1) + " is not 'outputname'");
                name = nameLine.Value;

                KeyValuePair<string, string> enabledLine = response[i * 3 + 2];
                if (enabledLine.Key == null)
                    throw new InvalidMpdResponseException("Invalid form of line " + (i * 3 + 2));
                if (!enabledLine.Key.Equals("outputenabled"))
                    throw new InvalidMpdResponseException("Key of line " + (i * 3 + 2) + " is not 'outputenabled'");
                if (!int.TryParse(enabledLine.Value, out enabled))
                    throw new InvalidMpdResponseException("Value of line " + (i * 3 + 2) + " is not a number");

                ret[i] = new MpdOutput(id, name, enabled > 0);
            }

            return ret;
        }
        /// <summary>
        /// Returns the list of tag types the MPD supports.
        /// </summary>
        /// <returns>The list of tag types the MPD supports.</returns>
        public string[] TagTypes()
        {
            MpdResponse response = _connection.Exec("tagtypes");

            string[] ret = new string[response.Message.Count];

            for (int i = 0; i < ret.Length; i++)
            {
                KeyValuePair<string, string> line = response[i];
                if (!line.Key.Equals("tagtype"))
                    throw new InvalidMpdResponseException("Key of line " + (i) + " is not 'tagtype'");
                ret[i] = line.Value;
            }

            return ret;
        }
        /// <summary>
        /// Starts an update of the MPD database.
        /// </summary>
        /// <returns>An sequential number of the update process.</returns>
        public int Update()
        {
            MpdResponse response = _connection.Exec("update");

            if (response.Message.Count != 1)
                throw new InvalidMpdResponseException("Respose message has more than one line.");

            int ret;

            KeyValuePair<string, string> line = response[0];
            if (!line.Key.Equals("updating_db"))
                throw new InvalidMpdResponseException("Key of line 0 is not 'updating_db'");
            if (!int.TryParse(line.Value, out ret))
                throw new InvalidMpdResponseException("Value of line 0 is not a number");

            return ret;
        }

        #endregion

        #region Database Commands
        /// <summary>
        /// Returns all files in the database who's attribute matches the given token. Works like the Search command but is case sensitive.
        /// </summary>
        /// <param name="scopeSpecifier">Specifies the attribute to search for.</param>
        /// <param name="token">The value the files attribute must have to be included in the result.</param>
        /// <returns>All files in the database who's attribute matches the given token.</returns>
        public List<MpdFile> Find(ScopeSpecifier scopeSpecifier, string token)
        {
            if (token == null)
                throw new ArgumentNullException("token");

            MpdResponse response = _connection.Exec("find", new string[] { TagConverter.ToTag(scopeSpecifier), token });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return MpdFile.buildList(response);
        }
        /// <summary>
        /// Returns all values found in files of the MPD for the given attribute.
        /// </summary>
        /// <param name="scopeSpecifier">The attribute who's values are requested.</param>
        /// <returns>All values found in files of the MPD for the given attribute.</returns>
        public List<string> List(ScopeSpecifier scopeSpecifier)
        {
            MpdResponse response = _connection.Exec("list", new string[] { TagConverter.ToTag(scopeSpecifier) });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return response.getValueList();
        }
        /// <summary>
        /// Returns all values for the given attribute found in files of the MPD where another attribute matches a given value.
        /// </summary>
        /// <param name="resultTag">The attribute whos values are returns.</param>
        /// <param name="searchTag">The attribute whos value should match a given value for the file to be included in the result.</param>
        /// <param name="searchValue">The value the searchTag attribute must match for the file to be included in the result.</param>
        /// <returns>All values found in files of the MPD for the given attribute.</returns>
        public List<string> List(ScopeSpecifier resultTag, ScopeSpecifier searchTag, string searchValue)
        {
            if (searchValue == null)
                throw new ArgumentNullException("searchValue");

            MpdResponse response = _connection.Exec("list", new string[] { TagConverter.ToTag(resultTag), TagConverter.ToTag(searchTag), searchValue });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return response.getValueList();
        }
        /// <summary>
        /// Returns the names of all files and directory found under the given path.
        /// </summary>
        /// <param name="path">The path whos subdirectories and their files are requested.</param>
        /// <returns>The names of all files and directory found under the given path.</returns>
        public List<string> ListAll(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            MpdResponse response = _connection.Exec("listall", new string[] { path });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return response.getValueList();
        }
        /// <summary>
        /// Returns the information of all files found in the given path and its subdirectories.
        /// </summary>
        /// <param name="path">The path of which the file information is requested.</param>
        /// <returns>The information of all files found in the given path and its subdirectories.</returns>
        public List<MpdFile> ListAllInfo(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            MpdResponse response = _connection.Exec("listallinfo", new string[] { path });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return MpdFile.buildList(response);
        }
        /// <summary>
        /// Returns the directory listing of the root directory.
        /// </summary>
        /// <returns>The listing of the root directory.</returns>
        public MpdDirectoryListing LsInfo()
        {
            return LsInfo(null);
        }
        /// <summary>
        /// Returns the directory listing of the given path.
        /// </summary>
        /// <param name="path">The path whos listing is requested.</param>
        /// <returns>The directory listing of the given path.</returns>
        public MpdDirectoryListing LsInfo(string path)
        {
            MpdResponse response;
            if (path == null)
                response = _connection.Exec("lsinfo");
            else
                response = _connection.Exec("lsinfo", new string[] { path });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return new MpdDirectoryListing(
                MpdFile.buildList(response),
                response.getAttributeValueList("directory"),
                response.getAttributeValueList("playlist"));
        }
        /// <summary>
        /// Returns all files in the database who's attribute matches the given token. Works like the Find command but is case insensitive.
        /// </summary>
        /// <param name="scopeSpecifier">Specifies the attribute to search for.</param>
        /// <param name="token">The value the files attribute must have to be included in the result.</param>
        /// <returns>All files in the database who's attribute matches the given token.</returns>
        public List<MpdFile> Search(ScopeSpecifier scopeSpecifier, string token)
        {
            if (token == null)
                throw new ArgumentNullException("token");

            MpdResponse response = _connection.Exec("search", new string[] { TagConverter.ToTag(scopeSpecifier), token });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return MpdFile.buildList(response);
        }

        #endregion

        #region  Playlist Commands
        /// <summary>
        /// Adds a file to the playlist.
        /// </summary>
        /// <param name="filename">The name and path of the file to add.</param>
        public void Add(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");

            MpdResponse response = _connection.Exec("add", new string[] { filename });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Adds a file to the playlist and returns the id.
        /// </summary>
        /// <param name="filename">The name and path of the file to add.</param>
        /// <rereturns>The id of the file in the playlist.</rereturns>
        public int AddId(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");

            MpdResponse response = _connection.Exec("add", new string[] { filename });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            if (response.Count != 1)
                throw new InvalidMpdResponseException("Returned more than one line for command addid.");

            string id = response["Id"];
            if (id == null)
                throw new InvalidMpdResponseException("Tag Id missing in response to command addid.");
            int tryId = -1;
            if (!int.TryParse(id, out tryId))
                throw new InvalidMpdResponseException("Tag Id in response to command addid does not contain an number.");

            return tryId;
        }
        /// <summary>
        /// Clears the playlist.
        /// </summary>
        public void Clear()
        {
            MpdResponse response = _connection.Exec("clear");

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Returns the information of the current song.
        /// </summary>
        /// <returns>The information of the current song.</returns>
        public MpdFile CurrentSong()
        {
            MpdResponse response = _connection.Exec("currentsong");

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return MpdFile.build(response);
        }
        /// <summary>
        /// Deletes the track with the given index from the current playlist.
        /// </summary>
        /// <param name="nr">The index of the track to remove from the playlist.</param>
        public void Delete(int nr)
        {
            MpdResponse response = _connection.Exec("delete", new string[] { nr.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Deletes the track with the given id from the current playlist.
        /// </summary>
        /// <param name="id">The id of the track to remove from the playlist.</param>
        public void DeleteId(int id)
        {
            MpdResponse response = _connection.Exec("deleteid", new string[] { id.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Loads the playlist with the given name.
        /// </summary>
        /// <param name="name">The name of the playlist to load.</param>
        public void Load(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            MpdResponse response = _connection.Exec("load", new string[] { name });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Renames a playlist.
        /// </summary>
        /// <param name="oldName">The old name of the playlist.</param>
        /// <param name="newName">The new name of the playlist.</param>
        public void Rename(string oldName, string newName)
        {
            if (oldName == null)
                throw new ArgumentNullException("oldName");
            if (newName == null)
                throw new ArgumentNullException("newName");

            MpdResponse response = _connection.Exec("rename", new string[] { oldName, newName });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Moves a track within the playlist.
        /// </summary>
        /// <param name="oldNr">The old index of the track in the playlist.</param>
        /// <param name="newNr">The new index of the track in the playlist.</param>
        public void Move(int oldNr, int newNr)
        {
            MpdResponse response = _connection.Exec("move", new string[] { oldNr.ToString(), newNr.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Moves a track within the playlist.
        /// </summary>
        /// <param name="id">The id of the track to move.</param>
        /// <param name="nr">The new index of the track in the playlist.</param>
        public void MoveId(int id, int nr)
        {
            MpdResponse response = _connection.Exec("moveid", new string[] { id.ToString(), nr.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Returns the meta data of the items in the current playlist.
        /// </summary>
        /// <returns>The meta data of the items in the current playlist.</returns>
        public List<MpdFile> PlaylistInfo()
        {
            MpdResponse response = _connection.Exec("playlistinfo");

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return MpdFile.buildList(response);
        }
        /// <summary>
        /// Returns the meta data of a track in the current playlist.
        /// </summary>
        /// <param name="nr">The index of the track in the playlist.</param>
        /// <returns>The meta data of the track in the current playlist.</returns>
        public MpdFile PlaylistInfo(int nr)
        {
            MpdResponse response = _connection.Exec("playlistinfo", new string[] { nr.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return MpdFile.build(response);
        }
        /// <summary>
        /// Returns the meta data of the items in the current playlist.
        /// </summary>
        /// <returns>The meta data of the items in the current playlist.</returns>
        public List<MpdFile> PlaylistId()
        {
            MpdResponse response = _connection.Exec("playlistid");

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return MpdFile.buildList(response);
        }
        /// <summary>
        /// Returns the meta data of a track in the current playlist.
        /// </summary>
        /// <param name="id">The id of the track in the playlist.</param>
        /// <returns>The meta data of the track in the current playlist.</returns>
        public MpdFile PlaylistId(int id)
        {
            MpdResponse response = _connection.Exec("playlistid", new string[] { id.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return MpdFile.build(response);
        }
        /// <summary>
        /// Returns all changed tracks in the playlist since the given version.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <returns>All changed songs in the playlist since the given version.</returns>
        public List<MpdFile> Plchanges(int version)
        {
            MpdResponse response = _connection.Exec("plchanges", new string[] { version.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return MpdFile.buildList(response);
        }
        /// <summary>
        /// Returns the ids and positions of the changed tracks in the playlist since the given version.
        /// </summary>
        /// <param name="version"></param>
        /// <returns>
        /// The ids and positions of the changed tracks in the playlist since the given version as KeyValuePairs. 
        /// The key is the index and the id is the value.
        /// </returns>
        public List<KeyValuePair<int, int>> PlChangesPosId(int version)
        {
            MpdResponse response = _connection.Exec("plchangesposid", new string[] { version.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            if (response.Count % 2 != 0)
                throw new InvalidMpdResponseException("Response to command plchangesposid contains an odd number of lines!");

            List<KeyValuePair<int, int>> ret = new List<KeyValuePair<int, int>>();

            for (int i = 0; i < response.Count; i += 2)
            {
                KeyValuePair<string, string> posLine = response[i];
                KeyValuePair<string, string> idLine = response[i + 1];

                if ((posLine.Key == null) || (posLine.Value == null))
                    throw new InvalidMpdResponseException("Invalid format of line " + i + "!");
                if ((idLine.Key == null) || (idLine.Value == null))
                    throw new InvalidMpdResponseException("Invalid format of line " + (i + 1) + "!");

                if (!posLine.Key.Equals("cpos"))
                    throw new InvalidMpdResponseException("Line " + i + " does not start with \"cpos\"!");
                if (!idLine.Key.Equals("Id"))
                    throw new InvalidMpdResponseException("Line " + (i + 1) + " does not start with \"Id\"!");

                int tryPos = -1;
                if (!int.TryParse(posLine.Value, out tryPos))
                    throw new InvalidMpdResponseException("Tag value on line " + i + " is not a number.");
                int tryId = -1;
                if (!int.TryParse(idLine.Value, out tryId))
                    throw new InvalidMpdResponseException("Tag value on line " + (i + 1) + " is not a number.");

                ret.Add(new KeyValuePair<int, int>(tryPos, tryId));
            }


            return ret;
        }
        /// <summary>
        /// Removes the playlist with the given name.
        /// </summary>
        /// <param name="name">The name of the playlist to remove.</param>
        public void Rm(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            MpdResponse response = _connection.Exec("rm", new string[] { name });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Saves the current playlist with the given name.
        /// </summary>
        /// <param name="name">The name to the save the currenty playlist.</param>
        public void Save(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            MpdResponse response = _connection.Exec("save", new string[] { name });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Shuffles the current playlist.
        /// </summary>
        public void Shuffle()
        {
            MpdResponse response = _connection.Exec("shuffle");

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Swaps the to tracks in the current playlist.
        /// </summary>
        /// <param name="nr1">The index of the first track.</param>
        /// <param name="nr2">The index of the second track.</param>
        public void Swap(int nr1, int nr2)
        {
            MpdResponse response = _connection.Exec("swap", new string[] { nr1.ToString(), nr2.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Swaps the to tracks in the current playlist.
        /// </summary>
        /// <param name="id1">The id of the first track.</param>
        /// <param name="id2">The id of the second track.</param>
        public void SwapId(int id1, int id2)
        {
            MpdResponse response = _connection.Exec("swapid", new string[] { id1.ToString(), id2.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Returns the filenames of the tracks in the given playlist.
        /// </summary>
        /// <param name="name">The playlist whos filename are requested.</param>
        /// <returns>The filenames of the tracks in the given playlist.</returns>
        public List<string> ListPlaylist(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            MpdResponse response = _connection.Exec("listplaylist", new string[] { name });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return response.getValueList();
        }
        /// <summary>
        /// Return the meta data of the tracks in the given playlist.
        /// </summary>
        /// <param name="name">The playlist whos files meta data are requested.</param>
        /// <returns>The meta data of the tracks in the given playlist.</returns>
        public List<MpdFile> ListPlaylistInfo(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            MpdResponse response = _connection.Exec("listplaylistinfo", new string[] { name });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return MpdFile.buildList(response);
        }
        /// <summary>
        /// Add a file to a playlist.
        /// </summary>
        /// <param name="name">The name of the playlist.</param>
        /// <param name="file">The path and name of the file to add.</param>
        public void PlaylistAdd(string name, string file)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (file == null)
                throw new ArgumentNullException("file");

            MpdResponse response = _connection.Exec("playlistadd", new string[] { name, file });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Clears all tracks from a playlist.
        /// </summary>
        /// <param name="name">The name of the playlist to clear.</param>
        public void PlaylistClear(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            MpdResponse response = _connection.Exec("playlistclear", new string[] { name });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Delete a file from a playlist.
        /// </summary>
        /// <param name="name">The name of the playlist</param>
        /// <param name="id">The id of the track to delete.</param>
        public void PlaylistDelete(string name, int id)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            MpdResponse response = _connection.Exec("playlistdelete", new string[] { name, id.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Moves a track in a playlist.
        /// </summary>
        /// <param name="name">The name of the playlist.</param>
        /// <param name="id">The id of the track to move.</param>
        /// <param name="nr">The position to move the track to.</param>
        public void PlaylistMove(string name, int id, int nr)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            MpdResponse response = _connection.Exec("playlistmove", new string[] { id.ToString(), nr.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Returns the meta data for all tracks in the current playlist whos attribute equals the given value.
        /// </summary>
        /// <param name="scopeSpecifier">The attribute to search for the given value.</param>
        /// <param name="token">The value to search for in the given attribute.</param>
        /// <returns>The meta data for all tracks in the current playlist whos attribute equals the given value.</returns>
        public List<MpdFile> PlaylistFind(ScopeSpecifier scopeSpecifier, string token)
        {
            if (token == null)
                throw new ArgumentNullException("token");

            MpdResponse response = _connection.Exec("playlistfind", new string[] { TagConverter.ToTag(scopeSpecifier), token });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return MpdFile.buildList(response);
        }
        /// <summary>
        /// Returns the meta data for all tracks in the current playlist whos attribute contains the given value.
        /// </summary>
        /// <param name="scopeSpecifier">The attribute to search for the given value.</param>
        /// <param name="token">The value to search for in the given attribute.</param>
        /// <returns>The meta data for all tracks in the current playlist whos attribute contains the given value.</returns>
        public List<MpdFile> PlaylistSearch(ScopeSpecifier scopeSpecifier, string token)
        {
            if (token == null)
                throw new ArgumentNullException("token");

            MpdResponse response = _connection.Exec("playlistsearch", new string[] { TagConverter.ToTag(scopeSpecifier), token });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return MpdFile.buildList(response);
        }
        #endregion

        #region  Playback Commands
        /// <summary>
        /// Sets the seconds to crossfade between songs.
        /// </summary>
        /// <param name="seconds">The seconds to crossfade between songs.</param>
        public void Crossfade(int seconds)
        {
            MpdResponse response = _connection.Exec("crossfade", new string[] { seconds.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Starts the playback of the next song in the playlist-
        /// </summary>
        public void Next()
        {
            MpdResponse response = _connection.Exec("next");

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Sets the MPD to pause or resume the playback.
        /// </summary>
        /// <param name="pause">If the playback should be paused or resumed.</param>
        public void Pause(bool pause)
        {
            MpdResponse response = _connection.Exec("pause", new string[] { pause ? "1" : "0" });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Starts the playback of the current item in the playlist.
        /// </summary>
        public void Play()
        {
            MpdResponse response = _connection.Exec("play");

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Starts the playback of the item with the given index in the playlist.
        /// </summary>
        /// <param name="nr">The index of the track in the playlist to start playing.</param>
        public void Play(int nr)
        {
            MpdResponse response = _connection.Exec("play", new string[] { nr.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Starts the playback of the track in the playlist with the id 0.
        /// </summary>
        public void PlayId()
        {
            MpdResponse response = _connection.Exec("playid");

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Starts the playback of the track in the playlist with the given id.
        /// </summary>
        /// <param name="id">The id of the track to start playing.</param>
        public void PlayId(int id)
        {
            MpdResponse response = _connection.Exec("playid", new string[] { id.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Starts the playback of the previous track in the playlist.
        /// </summary>
        public void Previous()
        {
            MpdResponse response = _connection.Exec("previous");

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Sets the MPD to random or sequential playback.
        /// </summary>
        /// <param name="random">If the MPD playlist should be played randomly.</param>
        public void Random(bool random)
        {
            MpdResponse response = _connection.Exec("random", new string[] { random ? "1" : "0" });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Sets if the MPD should repeat the playlist.
        /// </summary>
        /// <param name="repeat">If the MPD should repeat the playlist.</param>
        public void Repeat(bool repeat)
        {
            MpdResponse response = _connection.Exec("repeat", new string[] { repeat ? "1" : "0" });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Starts playback of a given song at the give position.
        /// </summary>
        /// <param name="nr">The index of the song in the playlist.</param>
        /// <param name="time">The number of seconds to start playback on.</param>
        public void Seek(int nr, int time)
        {
            MpdResponse response = _connection.Exec("seek", new string[] { nr.ToString(), time.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Starts playback of a given song at the give position.
        /// </summary>
        /// <param name="id">The id of the song in the playlist.</param>
        /// <param name="time">The number of seconds to start playback on.</param>
        public void SeekId(int id, int time)
        {
            MpdResponse response = _connection.Exec("seekid", new string[] { id.ToString(), time.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Sets the output volume of the MPD.
        /// </summary>
        /// <param name="vol">The output volume of the MPD between 0 and 100.</param>
        public void SetVol(int vol)
        {
            if (vol < 0)
                throw new ArgumentException("vol < 0");
            if (vol > 100)
                throw new ArgumentException("vol > 100");

            MpdResponse response = _connection.Exec("setvol", new string[] { vol.ToString() });

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }
        /// <summary>
        /// Stops the playback of the MPD.
        /// </summary>
        public void Stop()
        {
            MpdResponse response = _connection.Exec("stop");

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);
        }

        #endregion

        #region Misc Commands
        /// <summary>
        /// Clears the error message set in the MPD.
        /// </summary>
        public void ClearError()
        {
            _connection.Exec("clearerror");
        }
        /// <summary>
        /// Returns which commands the current user has access to. 
        /// </summary>
        /// <returns>The commands the current user has access to.</returns>
        public List<string> Commands()
        {
            MpdResponse response = _connection.Exec("commands");

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return response.getValueList();
        }
        /// <summary>
        /// Returns which commands the current user does has access to. 
        /// </summary>
        /// <returns>The commands the current user does has access to.</returns>
        public List<string> NotCommands()
        {
            MpdResponse response = _connection.Exec("notcommands");

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            return response.getValueList();
        }
        /// <summary>
        /// Send the password to the server allow access to the server if enabled in the MPD.
        /// </summary>
        /// <param name="password">The password to authorize to the server.</param>
        /// <returns>If the password is valid.</returns>
        public bool Password(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            return _connection.Exec("password", new string[] { password }).IsError;
        }
        /// <summary>
        /// Sends a ping command to the server and waits for the response.
        /// </summary>
        public void Ping()
        {
            _connection.Exec("ping");
        }
        /// <summary>
        /// Requests the current statistics from the MPD,
        /// </summary>
        /// <returns>The current statistics fromt the MPD.</returns>
        public MpdStatistics Stats()
        {
            MpdResponse response = _connection.Exec("stats");

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            int artists = -1;
            int albums = -1;
            int songs = -1;
            int uptime = -1;
            int playtime = -1;
            int db_playtime = -1;
            int db_update = -1;

            foreach (KeyValuePair<string, string> line in response)
            {
                if ((line.Key != null) && (line.Value != null))
                    switch (line.Key)
                    {
                        case "artists":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    artists = tryValue;
                            }
                            break;
                        case "albums":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    albums = tryValue;
                            }
                            break;
                        case "songs":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    songs = tryValue;
                            }
                            break;
                        case "uptime":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    uptime = tryValue;
                            }
                            break;
                        case "playtime":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    playtime = tryValue;
                            }
                            break;
                        case "db_playtime":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    db_playtime = tryValue;
                            }
                            break;
                        case "db_update":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    db_update = tryValue;
                            }
                            break;
                    }
            }

            return new MpdStatistics(artists, albums, songs, uptime, playtime, db_playtime, db_update);
        }
        /// <summary>
        /// Returns the current status of the MPD.
        /// </summary>
        /// <returns>The current status of the MPD.</returns>
        public MpdStatus Status()
        {
            MpdResponse response = _connection.Exec("status");

            if (response.IsError)
                throw new MpdResponseException(response.ErrorCode, response.ErrorMessage);

            int volume = -1;
            bool repeat = false;
            bool random = false;
            int playlist = -1;
            int playlistLength = -1;
            int playlistQueue = -1;
            int xFade = -1;
            MpdState state = MpdState.Unknown;
            int song = -1;
            int songId = -1;
            int timeElapsed = -1;
            int timeTotal = -1;
            int bitrate = -1;
            int audioSampleRate = -1;
            int audioBits = -1;
            int audioChannels = -1;
            int updatingDb = -1;
            string error = null;

            foreach (KeyValuePair<string, string> line in response)
            {
                if ((line.Key != null) && (line.Value != null))
                    switch (line.Key)
                    {
                        case "volume":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                {
                                    volume = tryValue;
                                    if (volume < 0)
                                        volume = 0;
                                    if (volume > 100)
                                        volume = 100;
                                }
                            }
                            break;
                        case "repeat":
                            repeat = (line.Value != null) && (line.Value.Equals("1"));
                            break;
                        case "random":
                            random = (line.Value != null) && (line.Value.Equals("1"));
                            break;
                        case "playlist":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    playlist = tryValue;
                            }
                            break;
                        case "playlistlength":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    playlistLength = tryValue;
                            }
                            break;
                        case "playlistqueue":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    playlistQueue = tryValue;
                            }
                            break;
                        case "xfade":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    xFade = tryValue;
                            }
                            break;
                        case "state":
                            switch (line.Value)
                            {
                                case "play":
                                    state = MpdState.Play;
                                    break;
                                case "pause":
                                    state = MpdState.Pause;
                                    break;
                                case "stop":
                                    state = MpdState.Stop;
                                    break;
                            }
                            break;
                        case "song":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    song = tryValue;
                            }
                            break;
                        case "songid":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    songId = tryValue;
                            }
                            break;
                        case "time":
                            int index = line.Value.IndexOf(':');
                            if (index >= 0)
                            {
                                int tryValue;
                                if (int.TryParse(line.Value.Substring(0, index), out tryValue))
                                    timeElapsed = tryValue;
                                if (int.TryParse(line.Value.Substring(index + 1), out tryValue))
                                    timeTotal = tryValue;
                            }
                            break;
                        case "bitrate":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    bitrate = tryValue;
                            }
                            break;
                        case "audio":
                            Match match = STATUS_AUDIO_REGEX.Match(line.Value);
                            if (match.Success)
                            {
                                int tryValue;
                                if (int.TryParse(match.Result("$sampleRate"), out tryValue))
                                    audioSampleRate = tryValue;
                                if (int.TryParse(match.Result("$bits"), out tryValue))
                                    audioBits = tryValue;
                                if (int.TryParse(match.Result("$channels"), out tryValue))
                                    audioChannels = tryValue;
                            }
                            break;
                        case "updating_db":
                            {
                                int tryValue;
                                if (int.TryParse(line.Value, out tryValue))
                                    updatingDb = tryValue;
                            }
                            break;
                        case "error":
                            error = line.Value;
                            break;
                    }
            }

            return new MpdStatus(
                volume,
                repeat,
                random,
                playlist,
                playlistLength,
                xFade,
                state,
                song,
                songId,
                timeElapsed,
                timeTotal,
                bitrate,
                audioSampleRate,
                audioBits,
                audioChannels,
                updatingDb,
                error
                );
        }

        #endregion
    }
}
