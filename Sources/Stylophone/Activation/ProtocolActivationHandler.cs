using System;
using System.Threading.Tasks;
using Stylophone.Common.Interfaces;
using Windows.ApplicationModel.Activation;
using CommunityToolkit.Mvvm.DependencyInjection;
using Stylophone.Common.Services;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Status;
using MpcNET.Commands.Playlist;
using Stylophone.Common.ViewModels;
using Windows.UI.Popups;

namespace Stylophone.Activation
{
    internal class ProtocolActivationHandler : ActivationHandler<IActivatedEventArgs>
    {
        private MPDConnectionService _mpdService;

        public ProtocolActivationHandler(MPDConnectionService connectionService)
        {
            _mpdService = connectionService;
        }

        protected override async Task HandleInternalAsync(IActivatedEventArgs args)
        {
            ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;

            if (!_mpdService.IsConnected)
            {
                var host = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ServerHost));
                host = host?.Replace("\"", ""); // TODO: This is a quickfix for 1.x updates
                var port = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<int>(nameof(SettingsViewModel.ServerPort), 6600);
                var pass = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ServerPassword));
                _mpdService.SetServerInfo(host, port, pass);
                await _mpdService.InitializeAsync(false);
            }

            if (!_mpdService.IsConnected)
            {
                var dlg = new MessageDialog("Please open Stylophone and configure a MPD server.", "Couldn't connect to MPD server");
                await dlg.ShowAsync();
                return;
            }

            var status = _mpdService.CurrentStatus == MPDConnectionService.BOGUS_STATUS ? await _mpdService.SafelySendCommandAsync(new StatusCommand()) : _mpdService.CurrentStatus;

            // Protocol launches can do basic operations on the MPD server based on the verb
            // eg stylophone://?verb=stylophone_play
            var uri = new Uri(eventArgs.Uri.AbsoluteUri);
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);

            switch (queryDictionary["verb"])
            {
                case "stylophone_play":
                case "stylophone_pause":
                    await _mpdService.SafelySendCommandAsync(new PauseResumeCommand());
                    break;
                case "stylophone_stop":
                    await _mpdService.SafelySendCommandAsync(new StopCommand());
                    break;
                case "stylophone_next":
                    await _mpdService.SafelySendCommandAsync(new NextCommand());
                    break;
                case "stylophone_prev":
                    await _mpdService.SafelySendCommandAsync(new PreviousCommand());
                    break;
                case "stylophone_shuffle":
                    await _mpdService.SafelySendCommandAsync(new RandomCommand(!status.Random));
                    break;
                case "stylophone_volume_up":
                    await _mpdService.SafelySendCommandAsync(new SetVolumeCommand((byte)(status.Volume + 5)));
                    break;
                case "stylophone_volume_down":
                    await _mpdService.SafelySendCommandAsync(new SetVolumeCommand((byte)(status.Volume - 5)));
                    break;
                case "stylophone_volume_set":
                    var volume = queryDictionary["volume"] ?? "0";
                    await _mpdService.SafelySendCommandAsync(new SetVolumeCommand((byte)(int.Parse(volume))));
                    break;
                case "stylophone_seek":
                    var seek = int.Parse(queryDictionary["seek"] ?? "0");
                    await _mpdService.SafelySendCommandAsync(new SeekCurCommand(seek));
                    break;
                case "stylophone_load_playlist":
                    var playlist = queryDictionary["playlist"] ?? "";
                    await _mpdService.SafelySendCommandAsync(new LoadCommand(playlist));
                    break;
                default:
                    break;
            }
        }

        protected override bool CanHandleInternal(IActivatedEventArgs args)
        {
            return args.Kind == ActivationKind.Protocol;
        }
    }
}
