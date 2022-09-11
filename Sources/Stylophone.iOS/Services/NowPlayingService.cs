using System;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;
using MediaPlayer;
using MpcNET;
using MpcNET.Commands.Playback;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using Stylophone.Common.Helpers;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using UIKit;

namespace Stylophone.iOS.Services
{
    public class NowPlayingService
    {
        private MPNowPlayingInfo _nowPlayingInfo;
        private MPDConnectionService _mpdService;
        private IApplicationStorageService _storageService;

        private AVAudioPlayer _silencePlayer;

        public NowPlayingService(MPDConnectionService mpdService, IApplicationStorageService storageService)
        {
            _mpdService = mpdService;
            _storageService = storageService;

            // https://stackoverflow.com/questions/48289037/using-mpnowplayinginfocenter-without-actually-playing-audio
            // TODO This breaks when LibVLC playback stops
            _silencePlayer = new AVAudioPlayer(new NSUrl("silence.wav",false,NSBundle.MainBundle.ResourceUrl), null, out var error);
            _silencePlayer.NumberOfLoops = -1;
        }

        public void Initialize()
        {
            _nowPlayingInfo = MPNowPlayingInfoCenter.DefaultCenter.NowPlaying;

            _nowPlayingInfo.MediaType = MPNowPlayingInfoMediaType.Audio;
            _nowPlayingInfo.IsLiveStream = false;

            EnableCommands(_mpdService.IsConnected);
            RegisterCommands();

            // Hook up to the MPDConnectionService for status updates.
            _mpdService.ConnectionChanged += (s, e) => EnableCommands(_mpdService.IsConnected);
            _mpdService.StatusChanged += (s, e) => UpdateState(_mpdService.CurrentStatus);
        }

        private void EnableCommands(bool isConnected)
        {
            AVAudioSession.SharedInstance().SetActive(isConnected, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation);

            MPRemoteCommandCenter.Shared.PreviousTrackCommand.Enabled = isConnected;
            MPRemoteCommandCenter.Shared.NextTrackCommand.Enabled = isConnected;
            MPRemoteCommandCenter.Shared.ChangePlaybackPositionCommand.Enabled = isConnected;
            MPRemoteCommandCenter.Shared.TogglePlayPauseCommand.Enabled = isConnected;
            MPRemoteCommandCenter.Shared.ChangeRepeatModeCommand.Enabled = isConnected;
            MPRemoteCommandCenter.Shared.ChangeShuffleModeCommand.Enabled = isConnected;
            MPRemoteCommandCenter.Shared.StopCommand.Enabled = isConnected;
        }

        private void RegisterCommands()
        {
            MPRemoteCommandCenter.Shared.TogglePlayPauseCommand.AddTarget((evt) => {
                _mpdService.SafelySendCommandAsync(new PauseResumeCommand());
                return MPRemoteCommandHandlerStatus.Success;
            });

            MPRemoteCommandCenter.Shared.PreviousTrackCommand.AddTarget((evt) => {
                _mpdService.SafelySendCommandAsync(new PreviousCommand());
                return MPRemoteCommandHandlerStatus.Success;
            });

            MPRemoteCommandCenter.Shared.NextTrackCommand.AddTarget((evt) => {
                _mpdService.SafelySendCommandAsync(new NextCommand());
                return MPRemoteCommandHandlerStatus.Success;
            });

            MPRemoteCommandCenter.Shared.ChangeRepeatModeCommand.AddTarget((evt) => {
                var isRepeat = (evt.Command as MPChangeRepeatModeCommand).CurrentRepeatType;
                _mpdService.SafelySendCommandAsync(new RepeatCommand(isRepeat != MPRepeatType.Off));
                _mpdService.SafelySendCommandAsync(new SingleCommand(isRepeat == MPRepeatType.One));
                return MPRemoteCommandHandlerStatus.Success;
            });

            MPRemoteCommandCenter.Shared.ChangeShuffleModeCommand.AddTarget((evt) => {
                var isShuffle = (evt.Command as MPChangeShuffleModeCommand).CurrentShuffleType;
                _mpdService.SafelySendCommandAsync(new RandomCommand(isShuffle == MPShuffleType.Items));
                return MPRemoteCommandHandlerStatus.Success;
            });

            MPRemoteCommandCenter.Shared.ChangePlaybackPositionCommand.AddTarget((evt) => {
                var position = (evt as MPChangePlaybackPositionCommandEvent).PositionTime;

                position = Math.Round(position); // Fractional values don't seem to work well on iOS
                _mpdService.SafelySendCommandAsync(new SeekCurCommand(position));
                return MPRemoteCommandHandlerStatus.Success;
            });

            MPRemoteCommandCenter.Shared.StopCommand.AddTarget((evt) => {
                _mpdService.SafelySendCommandAsync(new StopCommand());
                return MPRemoteCommandHandlerStatus.Success;
            });
        }

        private void UpdateState(MpdStatus status)
        {
            switch (status.State)
            {
                case MpdState.Play:
                    _silencePlayer.Play(); 
                    _nowPlayingInfo.PlaybackRate = 1;
                    break;
                case MpdState.Pause:
                    _silencePlayer.Stop();
                    _nowPlayingInfo.PlaybackRate = 0;
                    break;
                case MpdState.Stop:
                    _silencePlayer.Stop();
                    _nowPlayingInfo.PlaybackRate = 0;
                    break;
                case MpdState.Unknown:
                    _silencePlayer.Stop();
                    _nowPlayingInfo.PlaybackRate = 0;
                    break;
                default:
                    break;
            }

            _nowPlayingInfo.ElapsedPlaybackTime = status.Elapsed.TotalSeconds;
            _nowPlayingInfo.PlaybackDuration = status.Duration.TotalSeconds;

            MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = _nowPlayingInfo;
        }

        public async Task UpdateMetadataAsync(TrackViewModel track)
        {
            try
            {
                _nowPlayingInfo.Artist = track.File.Artist ?? "";
                _nowPlayingInfo.Title = track.File.Title ?? "";
                _nowPlayingInfo.AlbumTitle = track.File.Album ?? "";

                var thumbnail = UIImage.FromBundle("AlbumPlaceholder");

                // Set the album art thumbnail.
                var uniqueIdentifier = Miscellaneous.GetFileIdentifier(track.File);

                // Use the cached albumart if it exists
                if (await _storageService.DoesFileExistAsync(uniqueIdentifier, "AlbumArt"))
                {
                    using (var fileStream = await _storageService.OpenFileAsync(uniqueIdentifier, "AlbumArt"))
                    using (var data = NSData.FromStream(fileStream))
                        thumbnail = UIImage.LoadFromData(data);
                }

                _nowPlayingInfo.Artwork = new MPMediaItemArtwork(thumbnail);

                MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = _nowPlayingInfo;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error while updating NowPlayingInfo: " + e);
            }
        }

        private void UpdateTimeline(TimeSpan current, TimeSpan length)
        {
            _nowPlayingInfo.ElapsedPlaybackTime = current.TotalSeconds;
            _nowPlayingInfo.PlaybackDuration = length.TotalSeconds;
            _nowPlayingInfo.PlaybackRate = 1;

            MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = _nowPlayingInfo;
        }
    }
}
