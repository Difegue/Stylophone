using System;
using System.Threading.Tasks;
using System.Windows.Input;

using FluentMPC.Helpers;
using FluentMPC.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET.Commands.Status;
using Windows.ApplicationModel;
using Windows.Storage;
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

        private ICommand _clearCacheCommand;

        public ICommand ClearCacheCommand
        {
            get
            {
                if (_clearCacheCommand == null)
                {
                    _clearCacheCommand = new RelayCommand(
                        async () =>
                        {
                            try
                            {
                                var cacheFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("AlbumArt");
                                await cacheFolder.DeleteAsync();
                                NotificationService.ShowInAppNotification("Cache has been deleted!");
                            } catch (Exception e)
                            {
                                NotificationService.ShowInAppNotification($"Something went wrong while deleting cache: {e}");
                            }
                            
                        });
                }

                return _clearCacheCommand;
            }
        }

        private ICommand _rescanDbCommand;

        public ICommand RescanDbCommand
        {
            get
            {
                if (_rescanDbCommand == null)
                {
                    _rescanDbCommand = new RelayCommand(
                        async () =>
                        {
                            if (MPDConnectionService.CurrentStatus.UpdatingDb > 0)
                            {
                                NotificationService.ShowInAppNotification("The database is already being updated.");
                                return;
                            }

                            try
                            {
                                using (var c = await MPDConnectionService.GetConnectionAsync())
                                {
                                    var res = await c.InternalResource.SendAsync(new MpcNET.Commands.Database.UpdateCommand());

                                    if (res.IsResponseValid)
                                        NotificationService.ShowInAppNotification("Database update started.");
                                    else
                                        NotificationService.ShowInAppNotification("Couldn't update DB: MPD response invalid.");
                                }
                            } catch (Exception e)
                            {
                                NotificationService.ShowInAppNotification($"Something went wrong while updating DB : {e}", 0);
                            }

                        });
                }

                return _rescanDbCommand;
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
                MPDConnectionService.ConnectionChanged += async (s, e) => await UpdateServerVersionAsync();

                // Initialize values directly to avoid calling CheckServerAddressAsync twice
                _serverHost =
                    await ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(ServerHost));

                _serverPort =
                    await ApplicationData.Current.LocalSettings.ReadAsync<int>(nameof(ServerPort));

                await CheckServerAddressAsync();
                VersionDescription = GetVersionDescription();

                _hasInstanceBeenInitialized = true;
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
