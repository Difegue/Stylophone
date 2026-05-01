using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.ViewModels
{
    public partial class LocalPlaybackViewModel : ViewModelBase
    {
        private IInteropService _interopService;
        private INotificationService _notificationService;
        private IPlaybackService _playbackService;
        private SettingsViewModel _settingsVm;
        private MPDConnectionService _mpdService;

        private string _serverHost;
        private int _serverPort;

        public LocalPlaybackViewModel(SettingsViewModel settingsVm, MPDConnectionService mpdService, IInteropService interopService, INotificationService notificationService, IPlaybackService playbackService, IDispatcherService dispatcherService) : base(dispatcherService)
        {
            _interopService = interopService;
            _notificationService = notificationService;
            _playbackService = playbackService;
            _settingsVm = settingsVm;
            _mpdService = mpdService;

            _volumeIcon = _interopService.GetIcon(PlaybackIcon.VolumeMute);

            // TODO this'd be better with an IMessenger + [NotifyPropertyChangedRecipients] in SettingsViewModel
            _settingsVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_settingsVm.IsLocalPlaybackEnabled))
                    IsEnabled = _settingsVm.IsLocalPlaybackEnabled;

                if (e.PropertyName == nameof(_settingsVm.ServerHost))
                    _serverHost = _settingsVm.ServerHost;

                if (e.PropertyName == nameof(_settingsVm.LocalPlaybackPort))
                    _serverPort = _settingsVm.LocalPlaybackPort;
            };
        }

        public void Initialize(string host, int port, bool isEnabled)
        {
            _serverHost = host;
            _serverPort = port;
            IsEnabled = isEnabled;
        }

        public void Stop()
        {
            if (IsEnabled && Volume != 0)
                IsPlaying = false;
        }

        public void Resume()
        {
            if (IsEnabled && Volume != 0)
                IsPlaying = true;
        }

        [ObservableProperty]
        private bool _isEnabled;

        [ObservableProperty]
        private string _volumeIcon;

        [ObservableProperty]
        private int _volume = 0;

        [ObservableProperty]
        private bool _isPlaying;

        partial void OnIsEnabledChanged(bool value)
        {
            if (!value)
            {
                // Reset state when local playback is disabled.
                IsPlaying = false;
                Volume = 0;
                _previousVolume = 10;
            }
        }

        partial void OnVolumeChanged(int value)
        {
            // If the user changed the volume, play the stream back
            if (!IsPlaying && value != 0)
                IsPlaying = true;

            _playbackService.Volume = value;

            if (value == 0)
            {
                VolumeIcon = _interopService.GetIcon(PlaybackIcon.VolumeMute);
            }
            else if (value < 25)
            {
                VolumeIcon = _interopService.GetIcon(PlaybackIcon.Volume25);
            }
            else if (value < 50)
            {
                VolumeIcon = _interopService.GetIcon(PlaybackIcon.Volume50);
            }
            else if (value < 75)
            {
                VolumeIcon = _interopService.GetIcon(PlaybackIcon.Volume75);
            }
            else
            {
                VolumeIcon = _interopService.GetIcon(PlaybackIcon.VolumeFull);
            }
        }

        partial void OnIsPlayingChanged(bool value)
        {
            try
            {
                if (value && _serverHost != null && _mpdService.IsConnected)
                {
                    var streamUrl = new Uri("http://" + _serverHost + ":" + _serverPort);
                    _playbackService.Play(streamUrl);
                }
                else
                {
                    _playbackService.Stop();
                }
            }
            catch (Exception e)
            {
                _notificationService.ShowInAppNotification(Resources.ErrorPlayingMPDStream, e.Message, NotificationType.Error);
            }
        }

        private int _previousVolume = 25;
        /// <summary>
        ///     Toggle if we should mute
        /// </summary>
        public void ToggleMute()
        {
            if (Volume > 0)
            {
                _previousVolume = Volume;
                IsPlaying = false;
                Volume = 0;
            }
            else
            {
                Volume = _previousVolume; // Setting MediaVolume automatically starts playback
            }
        }

    }
}
