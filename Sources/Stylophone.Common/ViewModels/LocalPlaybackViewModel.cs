using System;
using Stylophone.Common.Interfaces;

namespace Stylophone.Common.ViewModels
{
    public class LocalPlaybackViewModel : ViewModelBase
    {
        private IInteropService _interopService;
        private SettingsViewModel _settingsVm;

        private string _serverHost;

        public LocalPlaybackViewModel(SettingsViewModel settingsVm, IInteropService interopService, IDispatcherService dispatcherService): base(dispatcherService)
        {
            _interopService = interopService;
            _settingsVm = settingsVm;

            _volumeIcon = _interopService.GetIcon(PlaybackIcon.VolumeMute);

            _settingsVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_settingsVm.IsStreamingAvailable))
                    IsEnabled = _settingsVm.IsStreamingAvailable;

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
            if (IsPlaying)
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
    }
}
