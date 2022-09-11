using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using MpcNET;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Queue;
using MpcNET.Commands.Status;
using Stylophone.Common.Helpers;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stylophone.Common.ViewModels
{
    public abstract partial class PlaybackViewModelBase : ViewModelBase
    {

        private CancellationTokenSource _albumArtCancellationSource = new CancellationTokenSource();
        private CancellationTokenSource cts = new CancellationTokenSource();
        private List<Task> volumeTasks = new List<Task>();
        private List<Task> stateTasks = new List<Task>();

        protected INavigationService _navigationService;
        protected INotificationService _notificationService;
        private IInteropService _interop;
        private MPDConnectionService _mpdService;
        private TrackViewModelFactory _trackVmFactory;

        public LocalPlaybackViewModel LocalPlayback;

        public PlaybackViewModelBase(INavigationService navigationService, INotificationService notificationService, IDispatcherService dispatcherService, IInteropService interop,
            MPDConnectionService mpdService, TrackViewModelFactory trackVmFactory, LocalPlaybackViewModel localPlayback) :
            base(dispatcherService)
        {
            _navigationService = navigationService;
            _notificationService = notificationService;
            _dispatcherService = dispatcherService;
            _interop = interop;
            _mpdService = mpdService;
            _trackVmFactory = trackVmFactory;
            LocalPlayback = localPlayback;

            // Default to NowPlayingBar
            _hostType = VisualizationType.NowPlayingBar;
            _isTrackInfoAvailable = false;

            // Initialize icons
            _volumeIcon = _interop.GetIcon(PlaybackIcon.VolumeFull);
            _repeatIcon = _interop.GetIcon(PlaybackIcon.RepeatOff);
            _playButtonContent = _interop.GetIcon(PlaybackIcon.Play);

            // Bind the methods that we need
            _mpdService.StatusChanged += OnStateChange;
            _mpdService.SongChanged += OnTrackChange;

            _internalVolume = _mpdService.CurrentStatus.Volume;

            // Bind timer methods and start it
            _updateInformationTimer = new System.Timers.Timer(500);
            _updateInformationTimer.Elapsed += UpdateInformation;
            _updateInformationTimer.Start();

            // Update info to current track
            _mpdService.ConnectionChanged += OnConnectionChanged;

            if (_mpdService.IsConnected)
                Task.Run(() => InitializeAsync());
        }

        private void OnConnectionChanged(object sender, EventArgs e)
        {
            if (_mpdService.IsConnected)
            {
                Task.Run(() => InitializeAsync());
            }
            else
            {
                IsTrackInfoAvailable = false;
            }
        }

        private async Task InitializeAsync()
        {
            OnTrackChange(this, new SongChangedEventArgs { NewSongId = _mpdService.CurrentStatus.SongId });
            CurrentTimeValue = _mpdService.CurrentStatus.Elapsed.TotalSeconds;

            OnStateChange(this, null);
            await UpdateUpNextAsync(_mpdService.CurrentStatus);
        }

        #region Getters and Setters

        public bool HasNextTrack => NextTrack != null;

        /// <summary>
        /// The current playing track
        /// </summary>
        [ObservableProperty]
        private TrackViewModel _currentTrack;

        /// <summary>
        /// The next track in queue
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasNextTrack))]
        private TrackViewModel _nextTrack;

        [ObservableProperty]
        private bool _showTrackName;

        [ObservableProperty]
        private bool _isTrackInfoAvailable;

        [ObservableProperty]
        private VisualizationType _hostType;

        /// <summary>
        ///     The amount of time spent listening to the track
        /// </summary>
        [ObservableProperty]
        private string _timeListened = "00:00";

        /// <summary>
        ///     The amount of time remaining
        /// </summary>
        [ObservableProperty]
        private string _timeRemaining = "-00:00";

        /// <summary>
        ///     The current slider value
        /// </summary>
        [ObservableProperty]
        private double _currentTimeValue;

        /// <summary>
        ///     The max slider value
        /// </summary>
        [ObservableProperty]
        private double _maxTimeValue = 100;

        /// <summary>
        ///     The current text for the volume icon
        /// </summary>
        [ObservableProperty]
        private string _volumeIcon;

        /// <summary>
        ///     The current text for the repeat icon
        /// </summary>
        [ObservableProperty]
        private string _repeatIcon;

        /// <summary>
        ///     The content on the play_pause button
        /// </summary>
        [ObservableProperty]
        private string _playButtonContent;

        private double _internalVolume;
        /// <summary>
        ///     The current value of the volume slider
        /// </summary>
        public double MediaVolume
        {
            get => _internalVolume;
            set
            {
                // HACK: Abort if mpdService doesn't have updated volume from the server yet
                if (_mpdService.CurrentStatus == MPDConnectionService.BOGUS_STATUS)
                    return;

                SetProperty(ref _internalVolume, value);

                if (value > 0) // _previousVolume is only used to keep track of volume when muting, if the volume has changed from zero due to another client, the value becomes meaningless
                    _previousVolume = -1;

                // Cancel older volumeTasks
                cts.Cancel();
                cts = new CancellationTokenSource();

                // Set the volume
                if (_mpdService.CurrentStatus.Volume != value)
                    volumeTasks.Add(Task.Run(async () =>
                    {
                        await _mpdService.SafelySendCommandAsync(new SetVolumeCommand((byte)value));
                        Thread.Sleep(1000); // Wait for MPD to acknowledge the new volume in its status...

                        if (volumeTasks.Count == 1)
                            MediaVolume = _mpdService.CurrentStatus.Volume; // Update the value to the current server volume
                    }, cts.Token));

                // Update the UI
                if ((int)value == 0)
                {
                    VolumeIcon = _interop.GetIcon(PlaybackIcon.VolumeMute);
                }
                else if (value < 25)
                {
                    VolumeIcon = _interop.GetIcon(PlaybackIcon.Volume25);
                }
                else if (value < 50)
                {
                    VolumeIcon = _interop.GetIcon(PlaybackIcon.Volume50);
                }
                else if (value < 75)
                {
                    VolumeIcon = _interop.GetIcon(PlaybackIcon.Volume75);
                }
                else
                {
                    VolumeIcon = _interop.GetIcon(PlaybackIcon.VolumeFull);
                }

            }
        }
        /// <summary>
        ///     Are tracks shuffled
        /// </summary>
        [ObservableProperty]
        private bool _isShuffleEnabled;

        /// <summary>
        ///     Are tracks removed upon playback
        /// </summary>
        [ObservableProperty]
        private bool _isConsumeEnabled;

        /// <summary>
        ///     Is the song going to repeat when finished
        /// </summary>
        [ObservableProperty]
        private bool _isRepeatEnabled;

        /// <summary>
        ///     Is the song going to loop when finished
        /// </summary>
        [ObservableProperty]
        private bool _isSingleEnabled;

        partial void OnIsRepeatEnabledChanged(bool value)
        {
            if (value)
                RepeatIcon = _interop.GetIcon(PlaybackIcon.Repeat);
            else
                RepeatIcon = _interop.GetIcon(PlaybackIcon.RepeatOff);
        }

        partial void OnIsSingleEnabledChanged(bool value)
        {
            // Update UI icon
            if (value)
                RepeatIcon = _interop.GetIcon(PlaybackIcon.RepeatSingle);
            else
                RepeatIcon = _interop.GetIcon(PlaybackIcon.RepeatOff);
        }

        #endregion Getters and Setters

        #region Timer Methods

        /// <summary>
        ///     This timer runs every 500ms to ensure that the current position,
        ///     time, remaining time, etc. variables are correct.
        /// </summary>
        private readonly System.Timers.Timer _updateInformationTimer;

        /// <summary>
        ///     Timer method that is run to make sure the UI is kept up to date
        /// </summary>
        protected async void UpdateInformation(object sender, EventArgs e)
        {
            var status = _mpdService.CurrentStatus;

            // Only call the following if the player exists and the time is greater then 0.
            if (status.Elapsed.TotalMilliseconds <= 0)
                return;

            // Update song in case we went out of sync
            if (status.SongId != CurrentTrack?.File?.Id)
            {
                OnTrackChange(this, new SongChangedEventArgs { NewSongId = status.SongId });
                OnStateChange(this, null);
                await UpdateUpNextAsync(_mpdService.CurrentStatus);
            }

            if (!HasNextTrack)
                await UpdateUpNextAsync(status);

            if (CurrentTrack == null)
                return;        

            // Set the current time value - if the user isn't scrobbling the slider
            if (!_isUserMovingSlider)
            {
                CurrentTimeValue = status.Elapsed.TotalSeconds;

                // Set the time listened text
                TimeListened = Miscellaneous.FormatTimeString(status.Elapsed.TotalMilliseconds);
            }

            // Get the remaining time for the track
            var remainingTime = _mpdService.CurrentStatus.Duration.Subtract(status.Elapsed);

            // Set the time remaining text
            TimeRemaining = "-" + Miscellaneous.FormatTimeString(remainingTime.TotalMilliseconds);

            // Set the maximum value
            MaxTimeValue = status.Duration.TotalSeconds;
        }

        #endregion Timer Methods

        #region Track Control Methods

        /// <summary>
        /// Write the current queue to a playlist.
        /// </summary>
        public void SaveQueue() => Ioc.Default.GetService<QueueViewModel>().SaveQueueCommand.Execute(this);

        /// <summary>
        /// Clear the MPD queue.
        /// </summary>
        public void ClearQueue() => Ioc.Default.GetService<QueueViewModel>().ClearQueueCommand.Execute(this);

        /// <summary>
        ///     Toggle if the current track/playlist should repeat
        /// </summary>
        public void ToggleRepeat()
        {
            // Cancel older shuffleTasks
            cts.Cancel();
            cts = new CancellationTokenSource();

            // No option -> repeat on -> repeat and single on -> no option
            if (IsRepeatEnabled && IsSingleEnabled)
            {
                IsRepeatEnabled = false;
                IsSingleEnabled = false;
            }
            else
            if (IsRepeatEnabled && !IsSingleEnabled)
            {
                IsSingleEnabled = true;
            }
            else
            if (!IsRepeatEnabled && !IsSingleEnabled)
            {
                IsRepeatEnabled = true;
            }

            // Set the volume
            stateTasks.Add(Task.Run(async () =>
            {
                await _mpdService.SafelySendCommandAsync(new RepeatCommand(IsRepeatEnabled));
                await _mpdService.SafelySendCommandAsync(new SingleCommand(IsSingleEnabled));
                Thread.Sleep(1000); // Wait for MPD to acknowledge the new status...
            }, cts.Token));

        }

        /// <summary>
        ///     Toggle if the current playlist is shuffled
        /// </summary>
        public void ToggleShuffle()
        {
            // Cancel older shuffleTasks
            cts.Cancel();
            cts = new CancellationTokenSource();

            IsShuffleEnabled = !IsShuffleEnabled;

            // Set shuffle
            stateTasks.Add(Task.Run(async () =>
            {
                await _mpdService.SafelySendCommandAsync(new RandomCommand(IsShuffleEnabled));
                Thread.Sleep(1000); // Wait for MPD to acknowledge the new status...
                await UpdateUpNextAsync(_mpdService.CurrentStatus);
            }, cts.Token));
        }

        /// <summary>
        ///     Toggle if the current playlist has consume=1
        /// </summary>
        public void ToggleConsume()
        {
            // Cancel older shuffleTasks
            cts.Cancel();
            cts = new CancellationTokenSource();

            IsConsumeEnabled = !IsConsumeEnabled;

            // Set consume
            stateTasks.Add(Task.Run(async () =>
            {
                await _mpdService.SafelySendCommandAsync(new ConsumeCommand(IsConsumeEnabled));
                Thread.Sleep(1000); // Wait for MPD to acknowledge the new status...
            }, cts.Token));
        }

        private double _previousVolume = -1;
        /// <summary>
        ///     Toggle if we should mute
        /// </summary>
        public void ToggleMute()
        {
            // There's no mute status in MPD, so all we can do is remember the previous volume and set it to 0.
            if (_previousVolume < 0)
            {
                _previousVolume = MediaVolume;
                MediaVolume = 0;
            }
            else
            {
                MediaVolume = _previousVolume; // Setting MediaVolume automatically resets _previousVolume to -1
            }
        }

        public void NavigateNowPlaying()
        {
            _navigationService.Navigate<PlaybackViewModelBase>(this);
        }

        public bool IsFullScreen => _navigationService.CurrentPageViewModelType == typeof(PlaybackViewModelBase);

        #endregion Track Control Methods

        #region Track Playback State

        /// <summary>
        ///     Toggles the state between the track playing
        ///     and not playing
        /// </summary>
        public void ChangePlaybackState() => _ = _mpdService.SafelySendCommandAsync(new PauseResumeCommand());

        /// <summary>
        ///     Go forward one track
        /// </summary> 
        public void SkipNext()
        {
            // Immediately cancel album art loading if it's in progress to free up MPD server resources
            _albumArtCancellationSource.Cancel();
            _ = _mpdService.SafelySendCommandAsync(new NextCommand());
        }

        /// <summary>
        ///     Go backwards one track
        /// </summary>
        public void SkipPrevious()
        {
            // Immediately cancel album art loading if it's in progress to free up MPD server resources
            _albumArtCancellationSource.Cancel();
            _ = _mpdService.SafelySendCommandAsync(new PreviousCommand());
        }

        #endregion Track Playback State

        #region Methods

        private bool _isUserMovingSlider = false;
        public void OnPlayingSliderMoving()
        {
            _isUserMovingSlider = true;
        }

        public async void OnPlayingSliderChange()
        {
            if (CurrentTrack == null)
                return;

            // Update TimeListened/Remaining manually according to the new slider position
            var remainingTime = _mpdService.CurrentStatus.Duration.Subtract(TimeSpan.FromSeconds(CurrentTimeValue));
            TimeListened = Miscellaneous.FormatTimeString(CurrentTimeValue * 1000);
            TimeRemaining = "-" + Miscellaneous.FormatTimeString(remainingTime.TotalMilliseconds);

            // Set the track position
            CurrentTimeValue = Math.Round(CurrentTimeValue); // Fractional values don't seem to work well on iOS
            await _mpdService.SafelySendCommandAsync(new SeekCurCommand(CurrentTimeValue));

            // Wait for MPD Status to catch up before we start auto-updating the slider again
            var timer = new System.Timers.Timer(1000);
            timer.Elapsed += (sender, e) => _isUserMovingSlider = false;
            timer.AutoReset = false;
            timer.Start();
        }
        private async Task UpdateUpNextAsync(MpdStatus status)
        {
            var nextSongId = status.NextSongId;
            if (nextSongId != -1)
            {
                var response = await _mpdService.SafelySendCommandAsync(new PlaylistIdCommand(nextSongId));

                if (response != null)
                {
                    NextTrack = _trackVmFactory.GetTrackViewModel(response.First());
                }
            }
        }

        /// <summary>
        ///     Called when MPD loads a new track. Used
        ///     to update the required values for the UI.
        /// </summary>
        /// <param name="track"></param>
        private async void OnTrackChange(object sender, SongChangedEventArgs eventArgs)
        {
            // Same track or no track, no need to perform this logic
            if (eventArgs.NewSongId == CurrentTrack?.File?.Id)
                return;

            // Get song info from MPD
            var response = await _mpdService.SafelySendCommandAsync(new CurrentSongCommand());

            // Cancel previous track art load
            _albumArtCancellationSource.Cancel();
            _albumArtCancellationSource = new CancellationTokenSource();

            if (response != null)
            {
                IsTrackInfoAvailable = true;

                // Dispose previous track
                CurrentTrack?.Dispose();

                // Set the new current track
                CurrentTrack = _trackVmFactory.GetTrackViewModel(response);
            }
            else
            {
                // No response 
                IsTrackInfoAvailable = false;
            }

            if (CurrentTrack?.File != null)
            {
                TimeRemaining = "-" + Miscellaneous.FormatTimeString(CurrentTrack.File.Time / 1000);
                TimeListened = "00:00";
                CurrentTimeValue = 0;
                MaxTimeValue = CurrentTrack.File.Time;

                _ = Task.Run(async () =>
                {
                    await _interop.UpdateOperatingSystemIntegrationsAsync(CurrentTrack);
                    await CurrentTrack.GetAlbumArtAsync(HostType, _albumArtCancellationSource.Token);

                    // Re-update OS integrations, as we have album art now
                    if (!_albumArtCancellationSource.Token.IsCancellationRequested)
                        await _interop.UpdateOperatingSystemIntegrationsAsync(CurrentTrack);
                }).ConfigureAwait(false);

                await UpdateUpNextAsync(_mpdService.CurrentStatus);
            }
            else
            {
                // No track playing
                IsTrackInfoAvailable = false;
            }
        }

        private void OnStateChange(object sender, EventArgs eventArgs)
        {
            var status = _mpdService.CurrentStatus;

            // Remove completed requests
            volumeTasks.RemoveAll(t => t.IsCompleted);
            stateTasks.RemoveAll(t => t.IsCompleted);

            // Update volume to match the server value -- If we're not setting it ourselves
            if (volumeTasks.Count == 0)
            {
                MediaVolume = status.Volume;
            }

            // Ditto for shuffle/repeat/single
            if (stateTasks.Count == 0)
            {
                IsShuffleEnabled = status.Random;
                IsRepeatEnabled = status.Repeat;
                IsSingleEnabled = status.Single;
                IsConsumeEnabled = status.Consume;

                if (status.Single)
                    RepeatIcon = _interop.GetIcon(PlaybackIcon.RepeatSingle);
                else if (status.Repeat)
                    RepeatIcon = _interop.GetIcon(PlaybackIcon.Repeat);
                else
                    RepeatIcon = _interop.GetIcon(PlaybackIcon.RepeatOff);
            }

            switch (status.State)
            {
                case MpdState.Play:
                    IsTrackInfoAvailable = true;
                    PlayButtonContent = _interop.GetIcon(PlaybackIcon.Pause);
                    LocalPlayback.Resume();
                    break;

                case MpdState.Stop:
                    IsTrackInfoAvailable = true;
                    PlayButtonContent = _interop.GetIcon(PlaybackIcon.Play);
                    LocalPlayback.Stop();
                    break;

                case MpdState.Pause:
                    IsTrackInfoAvailable = true;
                    PlayButtonContent = _interop.GetIcon(PlaybackIcon.Play);
                    LocalPlayback.Stop();
                    break;

                default:
                    IsTrackInfoAvailable = false;
                    PlayButtonContent = _interop.GetIcon(PlaybackIcon.Play);
                    LocalPlayback.Stop();
                    break;
            }
        }

        #endregion Methods

        public virtual void Dispose()
        {
            // Unbind the methods that we need
            _mpdService.StatusChanged -= OnStateChange;
            _mpdService.SongChanged -= OnTrackChange;

            _updateInformationTimer.Stop();
            _updateInformationTimer.Dispose();
        }

        #region Commands

        [RelayCommand]
        private void AddToPlaylist(EventArgs obj)
        {
            // Track must exist
            if (CurrentTrack == null)
            {
                _notificationService.ShowInAppNotification(Resources.NotificationNoTrackPlaying, "", NotificationType.Warning);
            }

            CurrentTrack?.AddToPlaylistCommand.Execute(CurrentTrack.File);
        }

        [RelayCommand]
        private void ShowAlbum(EventArgs obj)
        {
            // Track must exist
            if (CurrentTrack == null)
            {
                _notificationService.ShowInAppNotification(Resources.NotificationNoTrackPlaying, "", NotificationType.Warning);
                return;
            }

            CurrentTrack.ViewAlbumCommand.Execute(CurrentTrack.File);
        }

        /// <summary>
        ///     Switch to compact overlay mode
        /// </summary>
        [RelayCommand]
        public abstract Task SwitchToCompactViewAsync(EventArgs obj);


        #endregion Method Bindings
    }
}
