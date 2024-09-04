using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LibVLCSharp.Shared;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.ViewModels
{
    public partial class LocalPlaybackViewModel : ViewModelBase
    {
        private IInteropService _interopService;
        private INotificationService _notificationService;
        private SettingsViewModel _settingsVm;
        private MPDConnectionService _mpdService;

        private LibVLC _vlcCore;
        private MediaPlayer _mediaPlayer;
        private string _serverHost;
        private int _serverPort;

        public LocalPlaybackViewModel(SettingsViewModel settingsVm, MPDConnectionService mpdService, IInteropService interopService, INotificationService notificationService, IDispatcherService dispatcherService) : base(dispatcherService)
        {
            _interopService = interopService;
            _notificationService = notificationService;
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

            // Run an idle loop in a spare thread to make sure the libVLC volume is always accurate
            // Workaround for UWP, see https://code.videolan.org/videolan/vlc/-/commit/6ea058bf2d0813dab247f973b2d7bc9804486d81
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (IsPlaying && _mediaPlayer != null && _mediaPlayer.Volume != _volume)
                            _mediaPlayer.Volume = _volume;

                        Thread.Sleep(500);
                    }
                    catch (Exception) { }
                }
            });
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
            if (value)
            {
                if (_vlcCore == null)
                    _vlcCore = new LibVLC();

                _mediaPlayer?.Dispose();
                
                _mediaPlayer = new MediaPlayer(_vlcCore);
            }
            else
            {
                // Reset 
                IsPlaying = false;
                Volume = 0;
                _previousVolume = 10;

                _vlcCore?.Dispose();
                _vlcCore = null;
            }
        }

        partial void OnVolumeChanged(int value)
        {
            // If the user changed the volume, play the stream back
            if (!IsPlaying && value != 0)
                IsPlaying = true;

            if (_mediaPlayer != null)
                _mediaPlayer.Volume = value;

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
                    var urlString = "http://" + _serverHost + ":" + _serverPort;
                    var streamUrl = new Uri(urlString);
                    var media = new Media(_vlcCore, streamUrl);

                    _mediaPlayer.Play(media);
                }
                else
                {
                    _mediaPlayer?.Stop();
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
