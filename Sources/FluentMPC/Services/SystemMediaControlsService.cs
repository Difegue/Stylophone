using System;
using System.Linq;
using System.Threading.Tasks;

using FluentMPC.Activation;
using FluentMPC.Helpers;
using FluentMPC.ViewModels.Items;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET;
using MpcNET.Commands.Playback;
using Windows.ApplicationModel.Activation;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;

namespace FluentMPC.Services
{
    public static class SystemMediaControlsService
    {

        private static MediaPlayer _mediaPlayer;
        private static SystemMediaTransportControls _smtc;

        public static void Initialize()
        {
            _mediaPlayer = new MediaPlayer();
            _smtc = _mediaPlayer.SystemMediaTransportControls;
            _mediaPlayer.CommandManager.IsEnabled = false;

            _smtc.IsPlayEnabled = true;
            _smtc.IsPauseEnabled = true;
            _smtc.IsNextEnabled = true;
            _smtc.IsPreviousEnabled = true;
            _smtc.IsChannelUpEnabled = true;
            _smtc.IsChannelDownEnabled = true;

            _smtc.ButtonPressed += SystemControls_ButtonPressed;
            _smtc.IsEnabled = MPDConnectionService.IsConnected;

            // Hook up to the MPDConnectionService for status updates.
            MPDConnectionService.ConnectionChanged += (s, e) => _smtc.IsEnabled = MPDConnectionService.IsConnected;
            MPDConnectionService.StatusChanged += (s,e) => UpdateState(MPDConnectionService.CurrentStatus);
        }

        

        private static async void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await MPDConnectionService.SafelySendCommandAsync(new PauseResumeCommand());
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await MPDConnectionService.SafelySendCommandAsync(new PauseResumeCommand());
                    break;
                case SystemMediaTransportControlsButton.Next:
                    await MPDConnectionService.SafelySendCommandAsync(new NextCommand());
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    await MPDConnectionService.SafelySendCommandAsync(new PreviousCommand());
                    break;
                case SystemMediaTransportControlsButton.ChannelDown:
                    await MPDConnectionService.SafelySendCommandAsync(new SetVolumeCommand((byte)(MPDConnectionService.CurrentStatus.Volume - 5)));
                    break;
                case SystemMediaTransportControlsButton.ChannelUp:
                    await MPDConnectionService.SafelySendCommandAsync(new SetVolumeCommand((byte)(MPDConnectionService.CurrentStatus.Volume + 5)));
                    break;
                default:
                    break;
            }
        }

        private static void UpdateState(MpdStatus status)
        {
            switch (status.State)
            {
                case MpdState.Play:
                    _smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MpdState.Pause:
                    _smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MpdState.Stop:
                    _smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
                case MpdState.Unknown:
                    _smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                default:
                    break;
            }

            UpdateTimeline(status.Elapsed, status.Duration);
        }

        public static async void UpdateMetadata(TrackViewModel track)
        {
            // Get the updater.
            SystemMediaTransportControlsDisplayUpdater updater = _smtc.DisplayUpdater;

            // Music metadata.
            updater.Type = MediaPlaybackType.Music;
            updater.MusicProperties.Artist = track.File.Artist;
            updater.MusicProperties.Title = track.File.Title;
            updater.MusicProperties.AlbumTitle = track.File.Album;

            // Set the album art thumbnail.
            var uniqueIdentifier = track.File.HasAlbum ? track.File.Album : track.File.HasTitle ? track.File.Title : track.File.Path;
            uniqueIdentifier = MiscHelpers.EscapeFilename(uniqueIdentifier);

            // Use the cached albumart if it exists
            var artUri = $"ms-appdata:///local/AlbumArt/{uniqueIdentifier}";

            StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("AlbumArt", CreationCollisionOption.OpenIfExists);

            if (!await pictureFolder.FileExistsAsync(uniqueIdentifier))
            {
                artUri = "ms-appx:///Assets/AlbumPlaceholder.png";
            }

            // RandomAccessStreamReference is defined in Windows.Storage.Streams
            updater.Thumbnail =
               RandomAccessStreamReference.CreateFromUri(new Uri(artUri));

            // Update the system media transport controls.
            updater.Update();
        }

        private static void UpdateTimeline(TimeSpan current, TimeSpan length)
        {
            // Create our timeline properties object 
            var timelineProperties = new SystemMediaTransportControlsTimelineProperties();

            // Fill in the data, using the media elements properties 
            timelineProperties.StartTime = TimeSpan.FromSeconds(0);
            timelineProperties.MinSeekTime = TimeSpan.FromSeconds(0);
            timelineProperties.Position = current;
            timelineProperties.MaxSeekTime = length;
            timelineProperties.EndTime = length;

            // Update the System Media transport Controls 
            _smtc.UpdateTimelineProperties(timelineProperties);
        }
    }
}
