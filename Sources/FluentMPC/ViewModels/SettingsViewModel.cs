using System;
using System.Threading.Tasks;
using System.Windows.Input;

using FluentMPC.Helpers;
using FluentMPC.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET.Commands.Status;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace FluentMPC.ViewModels
{

    public class SettingsViewModel : Observable
    {
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { Set(ref _elementTheme, value); }
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

            set {
                if (value != _serverHost)
                {
                    Task.Run(async () =>
                    {
                        await Windows.Storage.ApplicationData.Current.LocalSettings.SaveAsync(nameof(ServerHost), value ?? "localhost");
                        await CheckServerAddressAsync();
                    });
                }
                Set(ref _serverHost, value);
            }
        }

        private int _serverPort;

        public int ServerPort
        {
            get { return _serverPort; }

            set {
                if (value != _serverPort)
                {
                    Task.Run(async () =>
                    {
                        await Windows.Storage.ApplicationData.Current.LocalSettings.SaveAsync(nameof(ServerPort), value);
                        await CheckServerAddressAsync();
                    });   
                }
                Set(ref _serverPort, value);
            }
        }

        private bool _isCheckingServer;

        public bool IsCheckingServer
        {
            get { return _isCheckingServer; }

            set{ Set(ref _isCheckingServer, value);}
        }

        public bool IsServerValid => MPDConnectionService.IsConnected;

        private ICommand _switchThemeCommand;

        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                {
                    _switchThemeCommand = new RelayCommand<ElementTheme>(
                        async (param) =>
                        {
                            if (_hasInstanceBeenInitialized)
                            {
                                ElementTheme = param;
                                await ThemeSelectorService.SetThemeAsync(param);
                            }
                        });
                }

                return _switchThemeCommand;
            }
        }

        public SettingsViewModel()
        {
        }

        private bool _hasInstanceBeenInitialized = false;

        public async Task EnsureInstanceInitializedAsync()
        {
            if (!_hasInstanceBeenInitialized)
            {
                ServerHost =
                    await Windows.Storage.ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(ServerHost));

                ServerPort =
                    await Windows.Storage.ApplicationData.Current.LocalSettings.ReadAsync<int>(nameof(ServerPort));

                VersionDescription = GetVersionDescription();

                _hasInstanceBeenInitialized = true;

                MPDConnectionService.ConnectionChanged += async (s, e) => await UpdateServerVersionAsync();
            }
        }

        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private async Task CheckServerAddressAsync()
        {
            if (IsCheckingServer) return;

            await DispatcherHelper.ExecuteOnUIThreadAsync(() => IsCheckingServer = true);

            await MPDConnectionService.InitializeAsync();
            await DispatcherHelper.ExecuteOnUIThreadAsync(() => {
                OnPropertyChanged(nameof(IsServerValid));
                IsCheckingServer = false;
            } );
        }
        private async Task UpdateServerVersionAsync()
        {
            if (!MPDConnectionService.IsConnected) return;

            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var response = await c.InternalResource.SendAsync(new StatsCommand());

                if (response.IsResponseValid)
                {
                    var stats = response.Response.Content;
                    var db_update = int.Parse(stats["db_update"]);
                    var lastUpdatedDb = DateTimeOffset.FromUnixTimeSeconds(db_update).UtcDateTime;

                    // Build info string
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    ServerInfo = $"MPD Protocol {c.InternalResource.Version}\n" +
                                 $"{stats["songs"]} Songs, {stats["albums"]} Albums\n" +
                                 $"Database last updated {lastUpdatedDb}");
                }
            }
        }
    }
}
