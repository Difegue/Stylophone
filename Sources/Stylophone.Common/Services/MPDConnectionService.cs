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
using MpcNET.Commands.Reflection;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.Services
{
    public class SongChangedEventArgs : EventArgs { public int NewSongId { get; set; } }

    public class MPDConnectionService
    {
        private const int ConnectionPoolSize = 5;
        public static MpdStatus BOGUS_STATUS = new MpdStatus(-1, false, false, false, false, -1, -1, -1, MpdState.Unknown, -1, -1, -1, -1, TimeSpan.Zero, TimeSpan.Zero, -1, -1, -1, -1, -1, "", "");

        private INotificationService _notificationService;

        public MPDConnectionService(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public MpdStatus CurrentStatus { get; private set; } = BOGUS_STATUS;
        public string Version { get; private set; }

        // TODO: Needs a lock for thread safety
        public bool DisableQueueEvents { get; set; }

        public bool IsConnecting { get; private set; }
        public bool IsConnected { get; private set; }

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
        private CancellationTokenSource _cancelConnect;

        private string _host;
        private int _port;
        private string _pass;

        public void SetServerInfo(string host, int port, string pass)
        {
            _cancelConnect?.Cancel();
            _host = host;
            _port = port;
            _pass = pass;
        }

        public async Task InitializeAsync(bool withRetry = false)
        {
            IsConnecting = true;
            CurrentStatus = BOGUS_STATUS; // Reset status

            if (IsConnected)
            {
                IsConnected = false;
                ConnectionChanged?.Invoke(this, new EventArgs());
            }

            ClearResources();

            var cancelToken = _cancelConnect.Token;

            try
            {
                await TryConnecting(cancelToken);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Error while connecting: {e.Message}");

                ConnectionChanged?.Invoke(this, new EventArgs());

                if (withRetry && !cancelToken.IsCancellationRequested)
                {
                    // The RetryAttempter will call TryConnect() in five seconds.
                    _connectionRetryAttempter = new System.Timers.Timer(5000);
                    _connectionRetryAttempter.AutoReset = false;
                    _connectionRetryAttempter.Elapsed += async (s, e2) => await InitializeAsync(true);
                    _connectionRetryAttempter.Start();
                }
            }

            IsConnecting = false;
        }

        private void ClearResources()
        {
            _idleConnection?.SendAsync(new NoIdleCommand());
            _idleConnection?.DisconnectAsync();
            _statusConnection?.DisconnectAsync();

            _connectionRetryAttempter?.Stop();
            _connectionRetryAttempter?.Dispose();

            _statusUpdater?.Stop();
            _statusUpdater?.Dispose();

            _cancelIdle?.Cancel();
            _cancelIdle = new CancellationTokenSource();

            _cancelConnect?.Cancel();
            _cancelConnect = new CancellationTokenSource();

            ConnectionPool?.Clear();

            _idleConnection = null;
            _statusConnection = null;
        }

        private async Task TryConnecting(CancellationToken token)
        {
            if (token.IsCancellationRequested) return;
            if (!IPAddress.TryParse(_host, out var ipAddress)) 
                throw new Exception("Invalid IP address");

            _mpdEndpoint = new IPEndPoint(ipAddress, _port);

            _statusConnection = await GetConnectionInternalAsync(token);

            ConnectionPool = new ObjectPool<PooledObjectWrapper<MpcConnection>>(ConnectionPoolSize,
                async (poolToken) =>
                {
                    var c = await GetConnectionInternalAsync(poolToken);
                    return new PooledObjectWrapper<MpcConnection>(c)
                    {
                        // Check our internal global IsConnected status
                        OnValidateObject = (context) => IsConnected && !token.IsCancellationRequested,
                        OnReleaseResources = (conn) => conn?.Dispose()
                    };
                }
            );

            // Connected, initialize basic data
            Version = _statusConnection.Version;

            _idleConnection = await GetConnectionInternalAsync(_cancelIdle.Token);
            await UpdatePlaylistsAsync();
            InitializeStatusUpdater(_cancelIdle.Token);

            IsConnecting = false;
            IsConnected = true;
            ConnectionChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Get a raw MPD Connection. Please use <see cref="SafelySendCommandAsync{T}(IMpcCommand{T})"/> instead when possible.
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
            if (!IsConnected)
                return default(T);

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
                _notificationService.ShowInAppNotification(string.Format(Resources.ErrorSendingMPDCommand, command.GetType().Name), 
                    e.Message, NotificationType.Error);
            }

            return default(T);
        }

        private async Task<MpcConnection> GetConnectionInternalAsync(CancellationToken token = default)
        {
            var c = new MpcConnection(_mpdEndpoint);
            await c.ConnectAsync(token);

            // Handle password auth if there's a password defined
            if (!string.IsNullOrEmpty(_pass))
            {
                var r = await c.SendAsync(new PasswordCommand(_pass));
                if (!r.IsResponseValid)
                {
                    var mpdError = r.Response?.Result?.MpdError;
                    _notificationService.ShowInAppNotification(Resources.ErrorPassword, $"{mpdError ?? r.ToString()}", NotificationType.Error);
                }
            }

            return c;
        }

        private void InitializeStatusUpdater(CancellationToken token = default)
        {
            // Update status every second
            _statusUpdater?.Stop();
            _statusUpdater?.Dispose();
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
                        if (token.IsCancellationRequested || _idleConnection == null || !_idleConnection.IsConnected)
                            break;

                        var idleChanges = await _idleConnection.SendAsync(new IdleCommand("stored_playlist playlist player mixer output options update"));

                        if (idleChanges.IsResponseValid)
                            await HandleIdleResponseAsync(idleChanges.Response.Content);
                        else
                            throw new Exception(idleChanges.Response?.Content);
                    }
                    catch (Exception e)
                    {
                        if (token.IsCancellationRequested)
                            System.Diagnostics.Debug.WriteLine($"Idle connection cancelled.");
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in Idle connection thread: {e.Message}");
                            await InitializeAsync(true);
                        }
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

            if (subsystems.Contains("player") || subsystems.Contains("mixer") || subsystems.Contains("output") || subsystems.Contains("options") || subsystems.Contains("update"))
            {
                // Status have changed in a significant way
                await UpdateStatusAsync(_idleConnection, true);
                StatusChanged?.Invoke(this, new EventArgs());

                if (subsystems.Contains("player"))
                {
                    // Specifically, song has changed
                    SongChanged?.Invoke(this, new SongChangedEventArgs { NewSongId = CurrentStatus.SongId });
                }
            }
        }

        private bool _isUpdatingStatus = false;
        private async Task UpdateStatusAsync(MpcConnection connection, bool forceUpdate = false)
        {
            System.Diagnostics.Debug.WriteLine($"{ConnectionPool.ObjectsInPoolCount} connections in pool - {CurrentStatus}");

            if (_statusConnection == null) return;

            if (_isUpdatingStatus && !forceUpdate) return;

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
                    throw new Exception();
            }
            catch
            {
                await InitializeAsync(true);
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
                await InitializeAsync(true);
        }
    }
}
