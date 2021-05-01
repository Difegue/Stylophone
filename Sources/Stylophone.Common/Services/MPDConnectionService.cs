using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CodeProject.ObjectPool;
using MpcNET;
using MpcNET.Types;
using MpcNET.Commands.Playlist;
using MpcNET.Commands.Status;
using Stylophone.Common.Interfaces;

namespace Stylophone.Common.Services
{
    public class SongChangedEventArgs : EventArgs { public int NewSongId { get; set; } }

    public class MPDConnectionService
    {
        private const int ConnectionPoolSize = 5;
        private static MpdStatus BOGUS_STATUS = new MpdStatus(0, false, false, false, false, -1, -1, -1, MpdState.Unknown, -1, -1, -1, -1, TimeSpan.Zero, TimeSpan.Zero, -1, -1, -1, -1, -1, "", "");

        private INotificationService _notificationService;

        public MPDConnectionService(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public MpdStatus CurrentStatus { get; private set; } = BOGUS_STATUS;
        public string Version { get; private set; }

        // TODO: Needs a lock for thread safety
        public bool DisableQueueEvents { get; set; }

        private bool _connected;
        public bool IsConnected
        {
            get { return _connected; }

            set
            {
                _connected = value;
                ConnectionChanged?.Invoke(this, new EventArgs());

                // If IsConnected = false, the RetryAttempter will call TryConnect() every five seconds.
                _connectionRetryAttempter?.Stop();
                _connectionRetryAttempter?.Dispose();
                if (!value)
                {
                    _connectionRetryAttempter = new System.Timers.Timer(5000);
                    _connectionRetryAttempter.Elapsed += async (s, e) => await TryConnecting();
                    _connectionRetryAttempter.Start();
                }
            }
        }

        public List<MpdPlaylist> Playlists { get; private set; } = new List<MpdPlaylist>();
        public ObjectPool<PooledObjectWrapper<MpcConnection>> ConnectionPool;

        public event EventHandler<SongChangedEventArgs> SongChanged;
        public event EventHandler<EventArgs> StatusChanged;
        public event EventHandler<EventArgs> QueueChanged;
        public event EventHandler<EventArgs> PlaylistsChanged;
        public event EventHandler<EventArgs> ConnectionChanged;

        private MpcConnection _idleConnection;
        private MpcConnection _statusConnection;
        private IPEndPoint _mpdEndpoint;

        private System.Timers.Timer _statusUpdater;
        private System.Timers.Timer _connectionRetryAttempter;
        private CancellationTokenSource _cancelIdle;

        private string _host;
        private int _port;

        public async Task InitializeAsync(string host, int port)
        {
            _idleConnection?.SendAsync(new NoIdleCommand());
            _idleConnection?.DisconnectAsync();
            _statusConnection?.DisconnectAsync();

            _statusUpdater?.Stop();
            _statusUpdater?.Dispose();

            _cancelIdle?.Cancel();
            _cancelIdle = new CancellationTokenSource();

            _idleConnection = null;
            _statusConnection = null;
            _connected = false;

            _host = host;
            _port = port;

            await TryConnecting();
        }

        public async Task TryConnecting()
        {
            if (!IPAddress.TryParse(_host, out var ipAddress))
                return;

            try
            {
                _mpdEndpoint = new IPEndPoint(ipAddress, _port);
                _idleConnection = await GetConnectionInternalAsync();
                _statusConnection = await GetConnectionInternalAsync();

                ConnectionPool = new ObjectPool<PooledObjectWrapper<MpcConnection>>(ConnectionPoolSize,
                    async (token) =>
                    {
                        var c = await GetConnectionInternalAsync(token);
                        return new PooledObjectWrapper<MpcConnection>(c)
                        {
                            // Check our internal global IsConnected status
                            OnValidateObject = (context) => IsConnected,
                            OnReleaseResources = (conn) => conn?.DisconnectAsync()
                        };
                    }
                );

                // Connected, initialize basic data
                Version = _statusConnection.Version;
                await UpdatePlaylistsAsync();
                InitializeStatusUpdater(_cancelIdle.Token);
                IsConnected = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Error while connecting: {e.Message}");
                IsConnected = false;
            }
        }

        /// <summary>
        /// Get a raw MPD Connection. Please use <see cref="SafelySendCommandAsync{T}(IMpcCommand{T}, CoreDispatcher)"/> instead when possible.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<PooledObjectWrapper<MpcConnection>> GetConnectionAsync(CancellationToken token = default)
        {
            return await ConnectionPool.GetObjectAsync(token);
        }

        /// <summary>
        /// Send a command to the MPD server in an abstracted way. Shows notifications on screen if anything goes south.
        /// </summary>
        /// <typeparam name="T">Return type of the command</typeparam>
        /// <param name="command">IMpcCommand to send</param>
        /// <returns>The command results, or default value.</returns>
        public async Task<T> SafelySendCommandAsync<T>(IMpcCommand<T> command)
        {
            try
            {
                using (var c = await GetConnectionAsync())
                {
                    var response = await c.InternalResource.SendAsync(command);
                    if (!response.IsResponseValid)
                    {
                        // If we have an MpdError string, only show that as the error to avoid extra noise
                        var mpdError = response.Response?.Result?.MpdError;
                        if (mpdError != null && mpdError != "")
                            throw new Exception(mpdError);
                        else
                            throw new Exception($"Invalid server response: {response}.");
                    }

                    return response.Response.Content;
                }
            }
            catch (Exception e)
            {
                _notificationService.ShowInAppNotification($"Sending {command.GetType().Name} failed: {e.Message}", false);
            }

            return default(T);
        }

        private async Task<MpcConnection> GetConnectionInternalAsync(CancellationToken token = default)
        {
            var c = new MpcConnection(_mpdEndpoint);
            await c.ConnectAsync(token);
            return c;
        }

        private void InitializeStatusUpdater(CancellationToken token = default)
        {
            // Update status every second
            _statusUpdater = new System.Timers.Timer(1000);
            _statusUpdater.Elapsed += async (s, e) => await UpdateStatusAsync(_statusConnection);
            _statusUpdater.Start();

            // Run an idle loop in a spare thread to fire events when needed
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (token.IsCancellationRequested || _idleConnection == null)
                            break;

                        var idleChanges = await _idleConnection.SendAsync(new IdleCommand("stored_playlist playlist player mixer output options"));

                        if (idleChanges.IsResponseValid)
                            await HandleIdleResponseAsync(idleChanges.Response.Content);
                        else
                            IsConnected = false;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in Idle connection thread: {e.Message}");
                        IsConnected = false;
                    }
                }

            }).ConfigureAwait(false);
        }

        private async Task HandleIdleResponseAsync(string subsystems)
        {
            if (subsystems.Contains("playlist") && !DisableQueueEvents)
            {
                // Queue has changed
                QueueChanged?.Invoke(this, new EventArgs());
            }

            if (subsystems.Contains("stored_playlist"))
            {
                // m3u playlists have changed
                await UpdatePlaylistsAsync();
            }

            if (subsystems.Contains("player") || subsystems.Contains("mixer") || subsystems.Contains("output") || subsystems.Contains("options"))
            {
                // Status have changed in a significant way
                await UpdateStatusAsync(_idleConnection);
                StatusChanged?.Invoke(this, new EventArgs());

                if (subsystems.Contains("player"))
                {
                    // Specifically, song has changed
                    SongChanged?.Invoke(this, new SongChangedEventArgs { NewSongId = CurrentStatus.SongId });
                }
            }
        }

        private bool _isUpdatingStatus = false;
        private async Task UpdateStatusAsync(MpcConnection connection)
        {
            System.Diagnostics.Debug.WriteLine($"{ConnectionPool.ObjectsInPoolCount} connections in pool - {CurrentStatus}");

            if (_statusConnection == null || _isUpdatingStatus) return;

            _isUpdatingStatus = true;
            try
            {
                var response = await connection.SendAsync(new StatusCommand());

                if (response != null && response.IsResponseValid)
                {
                    var oldstatus = CurrentStatus;
                    CurrentStatus = response.Response.Content;

                    if (oldstatus == BOGUS_STATUS) // Clean up the default null status if the idle command hasn't done it for us yet 
                    {
                        StatusChanged?.Invoke(this, new EventArgs());
                        SongChanged?.Invoke(this, new SongChangedEventArgs { NewSongId = CurrentStatus.SongId });
                    }
                }
                else
                    IsConnected = false;
            }
            catch
            {
                IsConnected = false;
            }
            _isUpdatingStatus = false;
        }

        private async Task UpdatePlaylistsAsync()
        {
            var response = await _idleConnection.SendAsync(new ListPlaylistsCommand());

            if (response.IsResponseValid)
            {
                var playlists = response.Response.Content;

                Playlists.Clear();
                Playlists.AddRange(playlists);
                PlaylistsChanged?.Invoke(this, new EventArgs());
            }
            else
                IsConnected = false;
        }
    }
}
