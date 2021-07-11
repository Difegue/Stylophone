using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MpcNET.Commands.Output;
using MpcNET.Commands.Status;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.ViewModels
{

    public class SettingsViewModel : ViewModelBase
    {
        private IApplicationStorageService _applicationStorageService;
        private INotificationService _notificationService;
        private IDialogService _dialogService;
        private IInteropService _interop;
        private MPDConnectionService _mpdService;

        public SettingsViewModel(MPDConnectionService mpdService, IApplicationStorageService appStorage, INotificationService notificationService, IDispatcherService dispatcherService, IDialogService dialogService, IInteropService interop) :
            base(dispatcherService)
        {
            _mpdService = mpdService;
            _applicationStorageService = appStorage;
            _notificationService = notificationService;
            _dialogService = dialogService;
            _interop = interop;
        }

        public static new string GetHeader() => Resources.SettingsHeader;

        private bool _hasInstanceBeenInitialized = false;
        private int _previousUpdatingDb = 0;

        private Theme _elementTheme;
        public Theme ElementTheme
        {
            get { return _elementTheme; }
            set
            {
                if (value != _elementTheme)
                {
                    _applicationStorageService.SetValue(nameof(ElementTheme), value.ToString());
                }
                Set(ref _elementTheme, value);
            }
        }

        private string _versionDescription;
        public string VersionDescription
        {
            get { return _versionDescription; }
            set { Set(ref _versionDescription, value); }
        }

        private string _serverInfo;
        public string ServerInfo
        {
            get { return _serverInfo; }
            set { Set(ref _serverInfo, value); }
        }

        private string _serverHost;
        public string ServerHost
        {
            get { return _serverHost; }
            set
            {
                if (value != _serverHost)
                {
                    _applicationStorageService.SetValue(nameof(ServerHost), value ?? "localhost");
                    TriggerServerConnection(value, ServerPort, ServerPassword);
                }
                Set(ref _serverHost, value);
            }
        }

        private int _serverPort;
        public int ServerPort
        {
            get { return _serverPort; }
            set
            {
                if (value != _serverPort)
                {
                    _applicationStorageService.SetValue(nameof(ServerPort), value);
                    TriggerServerConnection(ServerHost, value, ServerPassword);
                }
                Set(ref _serverPort, value);
            }
        }

        private string _serverPass;
        public string ServerPassword
        {
            get { return _serverPass; }
            set
            {
                if (value != _serverPass)
                {
                    _applicationStorageService.SetValue(nameof(ServerPassword), value ?? "");
                    TriggerServerConnection(ServerHost, ServerPort, value);
                }
                Set(ref _serverPass, value);
            }
        }

        private bool _compactEnabled;
        public bool IsCompactSizing
        {
            get { return _compactEnabled; }
            set
            {
                if (value != _compactEnabled)
                {
                    _applicationStorageService.SetValue(nameof(IsCompactSizing), value);
                }
                Set(ref _compactEnabled, value);
            }
        }

        private bool _disableAnalytics;
        public bool DisableAnalytics
        {
            get { return _disableAnalytics; }
            set
            {
                if (value != _disableAnalytics)
                {
                    _applicationStorageService.SetValue(nameof(DisableAnalytics), value);
                }
                Set(ref _disableAnalytics, value);
            }
        }

        private bool _isCheckingServer;
        public bool IsCheckingServer
        {
            get { return _isCheckingServer; }
            set => Set(ref _isCheckingServer, value);
        }

        public bool IsServerValid => _mpdService.IsConnected;

        public string ServerStatus => IsServerValid ? ServerInfo.Split('\n').First() + (IsStreamingAvailable ? ", "+ Resources.SettingsLocalPlaybackAvailable : "") : Resources.SettingsNoServerError;

        private bool _httpdAvailable;
        public bool IsStreamingAvailable
        {
            get { return _httpdAvailable; }
            set => Set(ref _httpdAvailable, value);
        }

        private ICommand _switchThemeCommand;
        public ICommand SwitchThemeCommand => _switchThemeCommand ?? (_switchThemeCommand = new AsyncRelayCommand<Theme>(SwitchThemeAsync));

        private async Task SwitchThemeAsync(Theme param)
        {
            if (_hasInstanceBeenInitialized)
            {
                ElementTheme = param;
                await _interop.SetThemeAsync(param);
            }
        }

        private ICommand _switchSizingCommand;
        public ICommand SwitchSizingCommand => _switchSizingCommand ?? (_switchSizingCommand = new RelayCommand<string>(SwitchSizing));

        private void SwitchSizing(string param)
        {
            if (_hasInstanceBeenInitialized)
            {
                IsCompactSizing = bool.Parse(param);
            }
        }

        private ICommand _clearCacheCommand;
        public ICommand ClearCacheCommand => _clearCacheCommand ?? (_clearCacheCommand = new AsyncRelayCommand(ClearCacheAsync));

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

        private ICommand _rescanDbCommand;
        public ICommand RescanDbCommand => _rescanDbCommand ?? (_rescanDbCommand = new AsyncRelayCommand(RescanDbAsync));

        private async Task RescanDbAsync()
        {
            if (_mpdService.CurrentStatus.UpdatingDb > 0)
            {
                _notificationService.ShowInAppNotification(Resources.NotificationDbAlreadyUpdating);
                return;
            }

            var res = await _mpdService.SafelySendCommandAsync(new MpcNET.Commands.Database.UpdateCommand());

            if (res != null)
                _notificationService.ShowInAppNotification(Resources.NotificationDbUpdateStarted);
        }

        public async Task EnsureInstanceInitializedAsync()
        {
            if (!_hasInstanceBeenInitialized)
            {
                _mpdService.ConnectionChanged += async (s, e) => await UpdateServerVersionAsync();
                _mpdService.StatusChanged += async (s, e) => await CheckUpdatingDbAsync();

                // Initialize values directly to avoid calling CheckServerAddressAsync twice
                _compactEnabled = _applicationStorageService.GetValue<bool>(nameof(IsCompactSizing));
                _serverHost = _applicationStorageService.GetValue<string>(nameof(ServerHost));
                _serverPort = _applicationStorageService.GetValue<int>(nameof(ServerPort));
                _disableAnalytics = _applicationStorageService.GetValue<bool>(nameof(DisableAnalytics));
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

            return $"{appName} - {version.Major}.{version.Minor}.{(version.Build > -1 ? version.Build : 0)}.{(version.Revision > -1 ? version.Revision : 0)}";
        }

        private void TriggerServerConnection(string host, int port, string pass)
        {
            IsCheckingServer = true;
            _mpdService.SetServerInfo(host, port, pass);

            Task.Run(async () => await _mpdService.InitializeAsync());
        }

        private async Task UpdateServerVersionAsync()
        {
            IsCheckingServer = _mpdService.IsConnecting;

            await _dispatcherService.ExecuteOnUIThreadAsync(() => { OnPropertyChanged(nameof(IsServerValid)); OnPropertyChanged(nameof(ServerStatus)); });

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
                ServerInfo = $"MPD Protocol {_mpdService.Version}\n" +
                             $"{response["songs"]} Songs, {response["albums"]} Albums\n" +
                             $"Database last updated {lastUpdatedDb}";

                var outputs = await _mpdService.SafelySendCommandAsync(new OutputsCommand());

                if (outputs != null)
                {
                    var outputString = outputs.Select(o => o.Plugin).Aggregate((s, s2) => $"{s}, {s2}");
                    ServerInfo += $"\nOutputs available: {outputString}";

                    IsStreamingAvailable = outputs.Select(o => o.Plugin).Contains("httpd");
                }

                await _dispatcherService.ExecuteOnUIThreadAsync(() => { OnPropertyChanged(nameof(IsServerValid)); OnPropertyChanged(nameof(ServerStatus)); });
            }
        }
    }
}
