using System;
using System.Threading.Tasks;
using Stylophone.Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET;
using MpcNET.Commands.Playback;
using Stylophone.Common.Helpers;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Stylophone.Services
{
    public class SystemMediaControlsService
    {
        private SystemMediaTransportControls _smtc;
        private MPDConnectionService _mpdService;

        public SystemMediaControlsService(MPDConnectionService mpdService)
        {
            _mpdService = mpdService;
        }

        public void Initialize()
        {

                _smtc = SystemMediaTransportControls.GetForCurrentView();

                _smtc.IsPlayEnabled = true;
                _smtc.IsPauseEnabled = true;
                _smtc.IsNextEnabled = true;
                _smtc.IsPreviousEnabled = true;
                _smtc.IsChannelUpEnabled = true;
                _smtc.IsChannelDownEnabled = true;

                _smtc.ButtonPressed += SystemControls_ButtonPressed;
                _smtc.IsEnabled = _mpdService.IsConnected;

                // Hook up to the MPDConnectionService for status updates.
                _mpdService.ConnectionChanged += (s, e) => _smtc.IsEnabled = _mpdService.IsConnected;
                _mpdService.StatusChanged += (s, e) => UpdateState(_mpdService.CurrentStatus);
        }

        private async void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await _mpdService.SafelySendCommandAsync(new PauseResumeCommand());
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await _mpdService.SafelySendCommandAsync(new PauseResumeCommand());
                    break;
                case SystemMediaTransportControlsButton.Next:
                    await _mpdService.SafelySendCommandAsync(new NextCommand());
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    await _mpdService.SafelySendCommandAsync(new PreviousCommand());
                    break;
                case SystemMediaTransportControlsButton.ChannelDown:
                    await _mpdService.SafelySendCommandAsync(new SetVolumeCommand((byte)(_mpdService.CurrentStatus.Volume - 5)));
                    break;
                case SystemMediaTransportControlsButton.ChannelUp:
                    await _mpdService.SafelySendCommandAsync(new SetVolumeCommand((byte)(_mpdService.CurrentStatus.Volume + 5)));
                    break;
                default:
                    break;
            }
        }

        private void UpdateState(MpdStatus status)
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

        public async Task UpdateMetadataAsync(TrackViewModel track)
        {
            try
            {
                // Get the updater.
                SystemMediaTransportControlsDisplayUpdater updater = _smtc?.DisplayUpdater;

                if (updater == null) return;

                // Music metadata.
                updater.Type = MediaPlaybackType.Music;

                // SMTC doesn't like null values, so fallback to empty strings.
                updater.MusicProperties.Artist = track.File.Artist ?? "";
                updater.MusicProperties.Title = track.File.Title ?? "";
                updater.MusicProperties.AlbumTitle = track.File.Album ?? "";

                // Set the album art thumbnail.
                var uniqueIdentifier = track.File.HasAlbum ? track.File.Album : track.File.HasTitle ? track.File.Title : track.File.Path;
                uniqueIdentifier = Miscellaneous.EscapeFilename(uniqueIdentifier);

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
                updater?.Update();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error while updating SMTC: " + e);
            }
        }

        private void UpdateTimeline(TimeSpan current, TimeSpan length)
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
