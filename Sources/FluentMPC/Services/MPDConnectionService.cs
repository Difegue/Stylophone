using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CodeProject.ObjectPool;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using MpcNET;
using MpcNET.Types;
using Sundew.Base.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;

namespace FluentMPC.Services
{
    public class SongChangedEventArgs : EventArgs { public int NewSongId { get; set; } }

    public static class MPDConnectionService
    {
        // TODO: settings wireup
        private const string AddressKey = "ServerIPEndPoint";
        private const string PasswordKey = "ServerPassword";
        private const int ConnectionPoolSize = 10;

        public static MpdStatus CurrentStatus { get; private set; } = new MpdStatus(0, false, false, false, false, -1, -1, -1, MpdState.Unknown, -1, -1, -1, -1, TimeSpan.Zero, TimeSpan.Zero, -1, -1, -1, -1, -1, "");

        public static IList<MpdPlaylist> Playlists { get; private set; } = new List<MpdPlaylist>();
        public static ObjectPool<PooledObjectWrapper<MpcConnection>> ConnectionPool;

        public static event EventHandler<SongChangedEventArgs> SongChanged;
        public static event EventHandler<EventArgs> StatusChanged;
        public static event EventHandler<EventArgs> PlaylistsChanged;
        public static event EventHandler<EventArgs> ConnectionLost;

        private static ThreadPoolTimer _statusUpdater;
        private static MpcConnection _connection;

        private static IPEndPoint _mpdEndpoint;

        public static async Task InitializeAsync()
        {
            try
            {
                IPAddress.TryParse("192.168.0.4", out var ipAddress);
                _mpdEndpoint = new IPEndPoint(ipAddress, 6600);
                _connection = await GetConnectionInternalAsync();

                ConnectionPool = new ObjectPool<PooledObjectWrapper<MpcConnection>>(ConnectionPoolSize,
                    async (t1,t2) =>
                    {
                        var c = await GetConnectionInternalAsync(t1);
                        return new PooledObjectWrapper<MpcConnection>(c)
                        {
                            OnReleaseResources = (c) => c.DisconnectAsync()
                        };
                    }
                );

            }
            catch (Exception e)
            {
                // TODO
            }

            // TODO move this to a onConnectionEstablished event
            InitializeStatusUpdater();
            UpdatePlaylistsAsync();
        }

        public static async Task<PooledObjectWrapper<MpcConnection>> GetConnectionAsync(CancellationToken token = default)
        {
            return await ConnectionPool.GetObjectAsync(token);
        }

        public static async Task<PooledObjectWrapper<MpcConnection>> GetAlbumArtConnectionAsync(CancellationToken token = default)
        {
            // Don't allocate extra connections for album art.
            while (ConnectionPool.ObjectsInPoolCount == 0)
            {
                if (token.IsCancellationRequested)
                    return null;
                Thread.Sleep(500);
            }

            return await ConnectionPool.GetObjectAsync(token);
        }

        private static async Task<MpcConnection> GetConnectionInternalAsync(CancellationToken token = default)
        {
            var c = new MpcConnection(_mpdEndpoint);

            if (token.IsCancellationRequested)
                return null; 

            await c.ConnectAsync(token);
            return c;
        }

        private static void InitializeStatusUpdater()
        {
            TimeSpan period = TimeSpan.FromSeconds(1);

            _statusUpdater = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                var response = await _connection.SendAsync(new MpcNET.Commands.Status.StatusCommand());

                if (response.IsResponseValid)
                {
                    var newStatus = response.Response.Content;
                    CompareAndFireEvents(CurrentStatus, newStatus);
                    CurrentStatus = newStatus;
                }
                else
                    ConnectionLost?.Invoke(Application.Current, new EventArgs()); //TODO handle reconnection attempts?

            }, period);
        }
        public async static void UpdatePlaylistsAsync()
        {
            var response = await _connection.SendAsync(new MpcNET.Commands.Playlist.ListPlaylistsCommand());

            if (response.IsResponseValid)
            {
                var playlists = response.Response.Content;

                if (!Playlists.SequenceEqual(playlists))
                {
                    Playlists.Clear();
                    Playlists.AddRange(playlists);
                    PlaylistsChanged?.Invoke(Application.Current, new EventArgs());
                }
            }
            else
                ConnectionLost?.Invoke(Application.Current, new EventArgs()); //TODO handle reconnection attempts?*/
        }

        private static void CompareAndFireEvents(MpdStatus currentStatus, MpdStatus newStatus)
        {
            if (currentStatus?.SongId != newStatus?.SongId)
            {
                SongChanged?.Invoke(Application.Current, new SongChangedEventArgs { NewSongId = newStatus.SongId });
            }

            // Fire the StatusChanged event if the status changed (which should happen pretty often..)
            if (!IsEqual(currentStatus,newStatus))
                StatusChanged?.Invoke(Application.Current, new EventArgs());
        }

        private static bool IsEqual(MpdStatus current, MpdStatus otherInput)
        {
            // wew lads
            if (current == null)
                return false;

            return ((current.Volume == otherInput.Volume) &&
                (current.Repeat == otherInput.Repeat) &&
                (current.Random == otherInput.Random) &&
                (current.Consume == otherInput.Consume) &&
                (current.Single == otherInput.Single) &&
                (current.Playlist == otherInput.Playlist) &&
                (current.State == otherInput.State) &&
                (current.Elapsed == otherInput.Elapsed) &&
                (current.Duration == otherInput.Duration) &&
                (current.Song == otherInput.Song) &&
                (current.SongId == otherInput.SongId) &&
                (current.UpdatingDb == otherInput.UpdatingDb) &&
                (current.NextSong == otherInput.NextSong) &&
                (current.NextSongId == otherInput.NextSongId));
        }

        public static async Task ReconnectServer()
        {
            _statusUpdater.Cancel();

            await _connection.DisconnectAsync();
            await _connection.ConnectAsync();

            InitializeStatusUpdater();
        }

        
    }
}
