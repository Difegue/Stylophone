using System;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.ViewModels
{
    public class LocalPlaybackViewModel : ViewModelBase
    {
        private IInteropService _interopService;
        private INotificationService _notificationService;
        private SettingsViewModel _settingsVm;
        private MPDConnectionService _mpdService;

        private string _serverHost;

        public LocalPlaybackViewModel(SettingsViewModel settingsVm, MPDConnectionService mpdService, IInteropService interopService, INotificationService notificationService, IDispatcherService dispatcherService): base(dispatcherService)
        {
            _interopService = interopService;
            _notificationService = notificationService;
            _settingsVm = settingsVm;
            _mpdService = mpdService;

            _volumeIcon = _interopService.GetIcon(PlaybackIcon.VolumeMute);

            _settingsVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_settingsVm.IsLocalPlaybackEnabled))
                    IsEnabled = _settingsVm.IsLocalPlaybackEnabled;

                if (e.PropertyName == nameof(_settingsVm.ServerHost))
                    _serverHost = _settingsVm.ServerHost;
            };
        }

        public void Initialize(string host, bool isEnabled)
        {
            _serverHost = host;
            IsEnabled = isEnabled;
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            private set
            {
                Set(ref _isEnabled, value);

                if (!value)
                {
                    // Reset 
                    IsPlaying = false;
                    Volume = 0;
                    _previousVolume = 10;
                }
                    
            }
        }

        private string _volumeIcon;
        /// <summary>
        ///     The current text for the volume icon
        /// </summary>
        public string VolumeIcon
        {
            get => _volumeIcon;
            private set => Set(ref _volumeIcon, value);
        }

        private double _volume = 0;
        public double Volume
        {
            get => _volume;
            set
            {
                Set(ref _volume, value);

                // If the user changed the volume, play the stream back
                if (!IsPlaying) 
                    IsPlaying = true;

                _interopService.SetStreamVolume(value);

                if ((int)value == 0)
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
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                Set(ref _isPlaying, value);
                UpdatePlayback();
            }
        }

        private double _previousVolume = 10;
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

        private void UpdatePlayback()
        {
            try
            {
                if (IsPlaying && _mpdService.IsConnected)
                {
                    var urlString = "http://" + _serverHost + ":8000/mpd.ogg";
                    var streamUrl = new Uri(urlString);
                    _interopService.PlayStream(streamUrl);
                }
                else
                {
                    _interopService.StopStream();
                }
            }
            catch (Exception e)
            {
                _notificationService.ShowInAppNotification(string.Format(Resources.ErrorPlayingMPDStream, e.Message), false);
            }
        }
    }
}
