using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CodeProject.ObjectPool;
using FluentMPC.Helpers;
using FluentMPC.ViewModels;
using MpcNET;
using MpcNET.Types;
using MpcNET.Commands.Playlist;
using MpcNET.Commands.Status;
using MpcNET.Commands.Queue;
using Sundew.Base.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;

namespace FluentMPC.Services
{
    public class SongChangedEventArgs : EventArgs { public int NewSongId { get; set; } }

    public static class MPDConnectionService
    {
        private const int ConnectionPoolSize = 5;
        private static MpdStatus BOGUS_STATUS = new MpdStatus(0, false, false, false, false, -1, -1, -1, MpdState.Unknown, -1, -1, -1, -1, TimeSpan.Zero, TimeSpan.Zero, -1, -1, -1, -1, -1, "", "");

        public static MpdStatus CurrentStatus { get; private set; } = BOGUS_STATUS;
        public static string Version { get; private set; }

        public static bool DisableQueueEvents { get; set; }

        private static bool _connected;

        public static bool IsConnected
        {
            get { return _connected; }

            set
            {
                _connected = value;
                ConnectionChanged?.Invoke(Application.Current, new EventArgs());

                // If IsConnected = false, the RetryAttempter will call TryConnect() every five seconds.
                _connectionRetryAttempter?.Cancel();
                if (!value)
                {
                    _connectionRetryAttempter = ThreadPoolTimer.CreatePeriodicTimer(async (source) => await TryConnecting(), TimeSpan.FromSeconds(5));
                }
            }
        }

        public static IList<MpdPlaylist> Playlists { get; private set; } = new List<MpdPlaylist>();
        public static ObjectPool<PooledObjectWrapper<MpcConnection>> ConnectionPool;

        public static event EventHandler<SongChangedEventArgs> SongChanged;
        public static event EventHandler<EventArgs> StatusChanged;
        public static event EventHandler<EventArgs> QueueChanged;
        public static event EventHandler<EventArgs> PlaylistsChanged;
        public static event EventHandler<EventArgs> ConnectionChanged;

        private static MpcConnection _idleConnection;
        private static MpcConnection _statusConnection;
        private static IPEndPoint _mpdEndpoint;

        private static ThreadPoolTimer _statusUpdater;
        private static ThreadPoolTimer _connectionRetryAttempter;
        private static CancellationTokenSource _cancelIdle;

        public static async Task InitializeAsync()
        {
            _idleConnection?.SendAsync(new NoIdleCommand());
            _idleConnection?.DisconnectAsync();
            _statusConnection?.DisconnectAsync();
            _statusUpdater?.Cancel();
            _cancelIdle?.Cancel();
            _cancelIdle = new CancellationTokenSource();

            _idleConnection = null;
            _statusConnection = null;
            _connected = false;

            await TryConnecting();
        }

        public static async Task TryConnecting()
        {
            try
            {
                IPAddress.TryParse(Singleton<SettingsViewModel>.Instance.ServerHost, out var ipAddress);
                _mpdEndpoint = new IPEndPoint(ipAddress, Singleton<SettingsViewModel>.Instance.ServerPort);
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
                            OnReleaseResources = (c) => c?.DisconnectAsync()
                        };
                    }
                );

                // Connected, initialize basic data
                Version = _statusConnection.Version;
                await UpdatePlaylistsAsync();
                InitializeStatusUpdater(_cancelIdle.Token);
                IsConnected = true;
            }
            catch (Exception)
            {
                IsConnected = false;
            }
        }

        /// <summary>
        /// Get a raw MPD Connection. Please use <see cref="SafelySendCommandAsync{T}(IMpcCommand{T}, CoreDispatcher)"/> instead when possible.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<PooledObjectWrapper<MpcConnection>> GetConnectionAsync(CancellationToken token = default)
        {
            return await ConnectionPool.GetObjectAsync(token);
        }

        /// <summary>
        /// Send a command to the MPD server in an abstracted way. Shows notifications on screen if anything goes south.
        /// </summary>
        /// <typeparam name="T">Return type of the command</typeparam>
        /// <param name="command">IMpcCommand to send</param>
        /// <param name="dispatcher">CoreDispatcher, needed if you're executing commands in a state where the dispatcher can be a secondary one</param>
        /// <returns>The command results, or default value.</returns>
        public static async Task<T> SafelySendCommandAsync<T>(IMpcCommand<T> command, CoreDispatcher dispatcher = null)
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
                        if (mpdError != null)
                            throw new Exception(mpdError);
                        else
                            throw new Exception($"Invalid server response: {response}.");
                    }

                    return response.Response.Content;
                }
            }
            catch (Exception e)
            {
                try
                {
                    if (dispatcher == null || dispatcher == CoreApplication.MainView.CoreWindow.Dispatcher) // Only invoke notificationservice on the main window
                        NotificationService.ShowInAppNotification($"Sending {command.GetType().Name} failed: {e.Message}", 0);
                } catch 
                {
                    // TODO: Dispatcher mismatch, we just swallow the exception for now.
                    System.Diagnostics.Debugger.Break();
                }
            }

            return default(T);
        }

        private static async Task<MpcConnection> GetConnectionInternalAsync(CancellationToken token = default)
        {
            var c = new MpcConnection(_mpdEndpoint);
            await c.ConnectAsync(token);
            return c;
        }

        private static void InitializeStatusUpdater(CancellationToken token = default)
        {
            // Update status every second
            _statusUpdater = ThreadPoolTimer.CreatePeriodicTimer(async (source) => await UpdateStatusAsync(_statusConnection), TimeSpan.FromSeconds(1));

            // Run an idle loop in a spare thread to fire events when needed
            Task.Run(async () =>
            {
               while (true)
                {
                    if (token.IsCancellationRequested || _idleConnection == null)
                        break;

                    var idleChanges = await _idleConnection.SendAsync(new IdleCommand("stored_playlist playlist player mixer output options"));

                    if (idleChanges.IsResponseValid)
                        await HandleIdleResponseAsync(idleChanges.Response.Content);
                    else
                        IsConnected = false;
               }

            });
        }

        private static async Task HandleIdleResponseAsync(string subsystems)
        {
            if (subsystems.Contains("playlist") && !DisableQueueEvents)
            {
                // Queue has changed
                QueueChanged?.Invoke(Application.Current, new EventArgs());
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
                StatusChanged?.Invoke(Application.Current, new EventArgs());

                if (subsystems.Contains("player"))
                {
                    // Specifically, song has changed
                    SongChanged?.Invoke(Application.Current, new SongChangedEventArgs { NewSongId = CurrentStatus.SongId });
                }
            }
        }

        private async static Task UpdateStatusAsync(MpcConnection connection)
        {
            System.Diagnostics.Debug.WriteLine($"{ConnectionPool.ObjectsInPoolCount} connections free in pool");

            if (_statusConnection == null) return;

            try
            {
                var response = await connection.SendAsync(new StatusCommand());

                if (response != null && response.IsResponseValid)
                {
                    var oldstatus = CurrentStatus;
                    CurrentStatus = response.Response.Content;

                    if (oldstatus == BOGUS_STATUS) // Clean up the default null status if the idle command hasn't done it for us yet 
                    {
                        StatusChanged?.Invoke(Application.Current, new EventArgs());
                        SongChanged?.Invoke(Application.Current, new SongChangedEventArgs { NewSongId = CurrentStatus.SongId });
                    }
                }
                else
                    IsConnected = false; 
            }
            catch
            {
                IsConnected = false;
            }
        }

        private async static Task UpdatePlaylistsAsync()
        {
            var response = await _idleConnection.SendAsync(new ListPlaylistsCommand());

            if (response.IsResponseValid)
            {
                var playlists = response.Response.Content;

                Playlists.Clear();
                Playlists.AddRange(playlists);
                PlaylistsChanged?.Invoke(Application.Current, new EventArgs());
            }
            else
                IsConnected = false; 
        }

        /// <summary>
        /// Basic method to get the current song. Independent and meant to be called by background tasks.
        /// </summary>
        /// <returns>The current song as a MpdFile. Throws if anything else happens 🤷</returns>
        public static async Task<IMpdFile> GetCurrentSong()
        {
            IPAddress.TryParse(Singleton<SettingsViewModel>.Instance.ServerHost, out var ipAddress);
            _mpdEndpoint = new IPEndPoint(ipAddress, Singleton<SettingsViewModel>.Instance.ServerPort);
            var connection = await GetConnectionInternalAsync();

            var response = await connection.SendAsync(new CurrentSongCommand());
            await connection.DisconnectAsync();

            if (response.IsResponseValid)
                return response.Response.Content;
            else
                return null;
        }
    }
}
