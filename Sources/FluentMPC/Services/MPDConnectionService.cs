using System;
using System.Net;
using System.Threading.Tasks;

using FluentMPC.Helpers;
using MpcNET;
using Windows.ApplicationModel.Core;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace FluentMPC.Services
{
    public class SongChangedEventArgs : EventArgs { public int NewSongId { get; set; } }

    public static class MPDConnectionService
    {
        // TODO: settings wireup
        private const string AddressKey = "ServerIPEndPoint";
        private const string PasswordKey = "ServerPassword";

        public static MpdStatus CurrentStatus { get; private set; } = new MpdStatus(0, false, false, false, false, -1, -1, -1, MpdState.Unknown, -1, -1, -1, -1, TimeSpan.Zero, TimeSpan.Zero, -1, -1, -1, -1, -1, "");

        public static event EventHandler<SongChangedEventArgs> SongChanged;
        public static event EventHandler<EventArgs> StatusChanged;
        public static event EventHandler<EventArgs> ConnectionLost;

        private static ThreadPoolTimer _statusUpdater;
        private static MpcConnection _connection;

        private static IPEndPoint _mpdEndpoint;

        public static async Task InitializeAsync()
        {
            IPAddress.TryParse("192.168.0.4", out var ipAddress);
            _mpdEndpoint = new IPEndPoint(ipAddress, 6600);

            _connection = await GetConnectionAsync();

            InitializeStatusUpdater();
        }

        public static async Task<MpcConnection> GetConnectionAsync()
        {
            var c = new MpcConnection(_mpdEndpoint);
            await c.ConnectAsync();
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
                    ConnectionLost?.Invoke(Application.Current, new EventArgs()); //TODO handle reconnection attempts

            }, period);
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

        /*public static async Task SetThemeAsync(ElementTheme theme)
        {
            Theme = theme;

            await SetRequestedThemeAsync();
            await SaveThemeInSettingsAsync(Theme);
        }

        public static async Task SetRequestedThemeAsync()
        {
            foreach (var view in CoreApplication.Views)
            {
                await view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (Window.Current.Content is FrameworkElement frameworkElement)
                    {
                        frameworkElement.RequestedTheme = Theme;
                    }
                });
            }
        }

        private static async Task<ElementTheme> LoadThemeFromSettingsAsync()
        {
            ElementTheme cacheTheme = ElementTheme.Default;
            string themeName = await ApplicationData.Current.LocalSettings.ReadAsync<string>(SettingsKey);

            if (!string.IsNullOrEmpty(themeName))
            {
                Enum.TryParse(themeName, out cacheTheme);
            }

            return cacheTheme;
        }

        private static async Task SaveThemeInSettingsAsync(ElementTheme theme)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync(SettingsKey, theme.ToString());
        }*/
    }
}
