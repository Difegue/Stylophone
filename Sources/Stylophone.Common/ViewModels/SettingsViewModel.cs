using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MpcNET.Commands.Output;
using MpcNET.Commands.Status;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.ViewModels
{

    public partial class SettingsViewModel : ViewModelBase
    {
        private IApplicationStorageService _applicationStorageService;
        private INotificationService _notificationService;
        private IInteropService _interop;
        private MPDConnectionService _mpdService;

        public SettingsViewModel(MPDConnectionService mpdService, IApplicationStorageService appStorage, INotificationService notificationService, IDispatcherService dispatcherService, IInteropService interop) :
            base(dispatcherService)
        {
            _mpdService = mpdService;
            _applicationStorageService = appStorage;
            _notificationService = notificationService;
            _interop = interop;
        }

        public static new string GetHeader() => Resources.SettingsHeader;

        private bool _hasInstanceBeenInitialized = false;
        private int _previousUpdatingDb = 0;

        [ObservableProperty]
        private Theme _elementTheme;

        [ObservableProperty]
        private string _versionDescription;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ServerStatus))]
        private string _serverInfo;

        [ObservableProperty]
        private string _serverHost;

        [ObservableProperty]
        private int _serverPort;

        [ObservableProperty]
        private string _serverPassword;

        [ObservableProperty]
        private bool _isCompactSizing;

        [ObservableProperty]
        private bool _isAlbumArtFetchingEnabled;

        [ObservableProperty]
        private bool _enableAnalytics;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsServerValid))]
        private bool _isCheckingServer;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ServerStatus))]
        private bool _isStreamingAvailable;

        [ObservableProperty]
        private bool _isLocalPlaybackEnabled;

        partial void OnElementThemeChanged(Theme value)
        {
            Task.Run (async () => await _interop.SetThemeAsync(value));

            if (value != _elementTheme)
            {
                _applicationStorageService.SetValue(nameof(ElementTheme), value.ToString());
            }
        }

        partial void OnServerHostChanged(string value)
        {
            _applicationStorageService.SetValue(nameof(ServerHost), value ?? "localhost");
            TriggerServerConnection(value, ServerPort, ServerPassword);
        }

        partial void OnServerPortChanged(int value)
        {
            _applicationStorageService.SetValue(nameof(ServerPort), value);
            TriggerServerConnection(ServerHost, value, ServerPassword);
        }

        partial void OnServerPasswordChanged(string value)
        {
            _applicationStorageService.SetValue(nameof(ServerPassword), value);
            TriggerServerConnection(ServerHost, ServerPort, value);
        }

        partial void OnIsCompactSizingChanged(bool value)
        {
            _applicationStorageService.SetValue(nameof(IsCompactSizing), value);
        }

        partial void OnIsAlbumArtFetchingEnabledChanged(bool value)
        {
            _applicationStorageService.SetValue(nameof(IsAlbumArtFetchingEnabled), value);
        }

        partial void OnEnableAnalyticsChanged(bool value)
        {
            _applicationStorageService.SetValue(nameof(EnableAnalytics), value);
        }

        public bool IsServerValid => _mpdService.IsConnected;
        public string ServerStatus => IsServerValid ? ServerInfo?.Split('\n')?.First() + (IsStreamingAvailable ? ", "+ Resources.SettingsLocalPlaybackAvailable : "") : 
            Resources.SettingsNoServerError;

        partial void OnIsLocalPlaybackEnabledChanged(bool value)
        {
            _applicationStorageService.SetValue(nameof(IsLocalPlaybackEnabled), value);
        }


        [RelayCommand]
        private async Task ClearCacheAsync()
        {
            try
            {
                await _applicationStorageService.DeleteFolderAsync("AlbumArt");
                _notificationService.ShowInAppNotification(Resources.NotificationCacheDeleted);
            }
            catch (Exception e)
            {
                _notificationService.ShowErrorNotification(e);
            }
        }

        [RelayCommand]
        private async Task RescanDbAsync()
        {
            if (_mpdService.CurrentStatus.UpdatingDb > 0)
            {
                _notificationService.ShowInAppNotification(Resources.NotificationDbAlreadyUpdating, "", NotificationType.Warning);
                return;
            }

            var res = await _mpdService.SafelySendCommandAsync(new MpcNET.Commands.Database.UpdateCommand());

            if (res != null)
                _notificationService.ShowInAppNotification(Resources.NotificationDbUpdateStarted);
        }


        [RelayCommand]
        private async Task RateAppAsync()
        {
            await _interop.OpenStoreReviewUrlAsync();
        }

        public async Task EnsureInstanceInitializedAsync()
        {
            if (!_hasInstanceBeenInitialized)
            {
                _mpdService.ConnectionChanged += async (s, e) => await UpdateServerVersionAsync();
                _mpdService.StatusChanged += async (s, e) => await CheckUpdatingDbAsync();

                // Initialize values directly to avoid calling CheckServerAddressAsync twice
                _isCompactSizing = _applicationStorageService.GetValue<bool>(nameof(IsCompactSizing));
                _serverHost = _applicationStorageService.GetValue<string>(nameof(ServerHost));
                _serverHost = _serverHost?.Replace("\"", ""); // TODO: This is a quickfix for 1.x updates

                _serverPort = _applicationStorageService.GetValue(nameof(ServerPort), 6600);
                _enableAnalytics = _applicationStorageService.GetValue(nameof(EnableAnalytics), true);
                _isAlbumArtFetchingEnabled = _applicationStorageService.GetValue(nameof(IsAlbumArtFetchingEnabled), true);
                _isLocalPlaybackEnabled = _applicationStorageService.GetValue<bool>(nameof(IsLocalPlaybackEnabled));

                Enum.TryParse(_applicationStorageService.GetValue<string>(nameof(ElementTheme)), out _elementTheme);

                await UpdateServerVersionAsync();
                VersionDescription = GetVersionDescription();

                _hasInstanceBeenInitialized = true;
            }
        }

        private async Task CheckUpdatingDbAsync()
        {
            var updatingDb = _mpdService.CurrentStatus.UpdatingDb;
            if (_previousUpdatingDb > 0 && _previousUpdatingDb != updatingDb && updatingDb == 0)
            {
                // A DB update job has concluded, refresh library
                await UpdateServerVersionAsync();
            }
            _previousUpdatingDb = updatingDb;
        }

        private string GetVersionDescription()
        {
            var appName = Resources.AppDisplayName;
            Version version = _interop.GetAppVersion();

            return $"{version.Major}.{version.Minor}.{(version.Revision > -1 ? version.Revision : 0)}";
        }

        public void RetryConnection() => TriggerServerConnection(ServerHost, ServerPort, ServerPassword);

        private void TriggerServerConnection(string host, int port, string pass)
        {
            IsCheckingServer = true;
            _mpdService.SetServerInfo(host, port, pass);

            Task.Run(async () => await _mpdService.InitializeAsync());
        }

        private async Task UpdateServerVersionAsync()
        {
            IsCheckingServer = _mpdService.IsConnecting;
            if (!_mpdService.IsConnected) return;

            var response = await _mpdService.SafelySendCommandAsync(new StatsCommand());

            if (response != null)
            {
                var lastUpdatedDb = DateTime.MinValue;

                if (response.ContainsKey("db_update"))
                {
                    var db_update = int.Parse(response["db_update"]);
                    lastUpdatedDb = DateTimeOffset.FromUnixTimeSeconds(db_update).UtcDateTime;
                }

                // Build info string
                var outputs = await _mpdService.SafelySendCommandAsync(new OutputsCommand());

                var songs = response.ContainsKey("songs") ? response["songs"] : "??";
                var albums = response.ContainsKey("albums") ? response["albums"] : "??";

                if (outputs != null)
                {
                    var outputString = outputs.Select(o => o.Plugin).Aggregate((s, s2) => $"{s}, {s2}");

                    ServerInfo = $"MPD Protocol {_mpdService.Version}\n" +
                             $"{songs} Songs, {albums} Albums\n" +
                             $"Database last updated {lastUpdatedDb}\n" +
                             $"Outputs available: {outputString}";

                    IsStreamingAvailable = outputs.Select(o => o.Plugin).Contains("httpd");

                    if (!IsStreamingAvailable)
                        IsLocalPlaybackEnabled = false;
                } 
                else
                {
                    ServerInfo = $"MPD Protocol {_mpdService.Version}\n" +
                             $"{songs} Songs, {albums} Albums\n" +
                             $"Database last updated {lastUpdatedDb}";
                }
            }
        }
    }
}
