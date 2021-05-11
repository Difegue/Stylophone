using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MpcNET;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
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
using System.Windows.Input;

namespace Stylophone.Common.ViewModels
{
    public abstract class PlaybackViewModelBase : ViewModelBase
    {

        private CancellationTokenSource _albumArtCancellationSource = new CancellationTokenSource();
        private CancellationTokenSource cts = new CancellationTokenSource();
        private List<Task> volumeTasks = new List<Task>();
        private List<Task> shuffleTasks = new List<Task>();

        private IDialogService _dialogService;
        protected INavigationService _navigationService;
        protected INotificationService _notificationService;
        private IInteropService _interop;
        private MPDConnectionService _mpdService;
        private TrackViewModelFactory _trackVmFactory;


        public PlaybackViewModelBase(IDialogService dialogService, INavigationService navigationService, INotificationService notificationService, IDispatcherService dispatcherService, IInteropService interop, MPDConnectionService mpdService, TrackViewModelFactory trackVmFactory):
            base(dispatcherService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _notificationService = notificationService;
            _dispatcherService = dispatcherService;
            _interop = interop;
            _mpdService = mpdService;
            _trackVmFactory = trackVmFactory;

            // Default to NowPlayingBar
            _hostType = VisualizationType.NowPlayingBar;
            _trackInfoAvailable = false;

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

        private TrackViewModel _currentTrack;
        /// <summary>
        /// The current playing track
        /// </summary>
        public TrackViewModel CurrentTrack
        {
            get => _currentTrack;
            private set => Set(ref _currentTrack, value);
        }

        private TrackViewModel _nextTrack;
        /// <summary>
        /// The next track in queue
        /// </summary>
        public TrackViewModel NextTrack
        {
            get => _nextTrack;
            private set
            {
                Set(ref _nextTrack, value);
                OnPropertyChanged(nameof(HasNextTrack));
            }
        }
        public bool HasNextTrack => NextTrack != null;

        private bool _showTrackName;
        public bool ShowTrackName
        {
            get => _showTrackName;
            set => Set(ref _showTrackName, value);
        }

        private bool _trackInfoAvailable;
        public bool IsTrackInfoAvailable
        {
            get => _trackInfoAvailable;
            set => Set(ref _trackInfoAvailable, value);
        }

        private VisualizationType _hostType;
        public VisualizationType HostType
        {
            get => _hostType;
            set
            {
                Set(ref _hostType, value);

                _albumArtCancellationSource.Cancel();
                _albumArtCancellationSource = new CancellationTokenSource();

                // (Re)Load CurrentTrack to take into account the new VisualizationType
                CurrentTrack?.GetAlbumArtAsync(HostType, _albumArtCancellationSource.Token);
            }
        }

        private string _timeListened = "00:00";
        /// <summary>
        ///     The amount of time spent listening to the track
        /// </summary>
        public string TimeListened
        {
            get => _timeListened;
            set => Set(ref _timeListened, value);
        }

        private string _timeRemaining = "-00:00";
        /// <summary>
        ///     The amount of time remaining
        /// </summary>
        public string TimeRemaining
        {
            get => _timeRemaining;
            set => Set(ref _timeRemaining, value);
        }

        private double _currentTimeValue;
        /// <summary>
        ///     The current slider value
        /// </summary>
        public double CurrentTimeValue
        {
            get => _currentTimeValue;
            set => Set(ref _currentTimeValue, value);
        }

        private double _maxTimeValue = 100;
        /// <summary>
        ///     The max slider value
        /// </summary>
        public double MaxTimeValue
        {
            get => _maxTimeValue;
            private set => Set(ref _maxTimeValue, value);
        }

        private string _volumeIcon = "\uE767";
        /// <summary>
        ///     The current text for the volume icon
        /// </summary>
        public string VolumeIcon
        {
            get => _volumeIcon;
            private set => Set(ref _volumeIcon, value);
        }

        private string _repeatIcon = "\uE8EE";
        /// <summary>
        ///     The current text for the repeat icon
        /// </summary>
        public string RepeatIcon
        {
            get => _repeatIcon;
            private set => Set(ref _repeatIcon, value);
        }

        private string _playButtonContent = "\uE769";
        /// <summary>
        ///     The content on the play_pause button
        /// </summary>
        public string PlayButtonContent
        {
            get => _playButtonContent;
            set => Set(ref _playButtonContent, value);
        }

        private double _internalVolume;
        /// <summary>
        ///     The current value of the volume slider
        /// </summary>
        public double MediaVolume
        {
            get => _internalVolume;
            set
            {
                Set(ref _internalVolume, value);

                if (value > 0) // _previousVolume is only used to keep track of volume when muting, if the volume has changed from zero due to another client, the value becomes meaningless
                    _previousVolume = -1;

                // Cancel older volumeTasks
                cts.Cancel();
                cts = new CancellationTokenSource();

                // Set the volume
                volumeTasks.Add(Task.Run(async () =>
                {
                    await _mpdService.SafelySendCommandAsync(new SetVolumeCommand((byte)value));
                    Thread.Sleep(1000); // Wait for MPD to acknowledge the new volume in its status...
                }, cts.Token));

                // Update the UI
                if ((int)value == 0)
                {
                    VolumeIcon = "\uE74F";
                }
                else if (value < 25)
                {
                    VolumeIcon = "\uE992";
                }
                else if (value < 50)
                {
                    VolumeIcon = "\uE993";
                }
                else if (value < 75)
                {
                    VolumeIcon = "\uE994";
                }
                else
                {
                    VolumeIcon = "\uE767";
                }

            }
        }

        private bool _isShuffledEnabled;
        /// <summary>
        ///     Are tracks shuffled
        /// </summary>
        public bool IsShuffleEnabled
        {
            get => _isShuffledEnabled;
            set => Set(ref _isShuffledEnabled, value);
        }

        private bool _isRepeatEnabled;
        /// <summary>
        ///     Is the song going to repeat when finished
        /// </summary>
        public bool IsRepeatEnabled
        {
            get => _isRepeatEnabled;
            set => Set(ref _isRepeatEnabled, value);
        }

        private bool _isSingleEnabled;
        /// <summary>
        ///     Is the song going to loop when finished
        /// </summary>
        public bool IsSingleEnabled
        {
            get => _isSingleEnabled;
            set
            {
                Set(ref _isSingleEnabled, value);

                // Update UI icon
                if (value)
                    RepeatIcon = "\uE8ED";
                else
                    RepeatIcon = "\uE8EE";
            }
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
            // Only call the following if the player exists and the time is greater then 0.
            if (_mpdService.CurrentStatus.Elapsed.TotalMilliseconds <= 0)
                return;

            if (CurrentTrack == null)
                return;

            if (!HasNextTrack)
                await UpdateUpNextAsync(_mpdService.CurrentStatus);

            await _dispatcherService.ExecuteOnUIThreadAsync(() =>
            {
                var status = _mpdService.CurrentStatus;

                // Set the current time value - if the user isn't scrobbling the slider
                if (!_isUserMovingSlider)
                    CurrentTimeValue = status.Elapsed.TotalSeconds;

                // Get the remaining time for the track
                var remainingTime = _mpdService.CurrentStatus.Duration.Subtract(status.Elapsed);

                // Set the time listened text
                TimeListened = Miscellaneous.FormatTimeString(status.Elapsed.TotalMilliseconds);

                // Set the time remaining text
                TimeRemaining = "-" + Miscellaneous.FormatTimeString(remainingTime.TotalMilliseconds);

                // Set the maximum value
                MaxTimeValue = status.Duration.TotalSeconds;
            });
        }

        #endregion Timer Methods

        #region Track Control Methods

        /// <summary>
        /// Write the current queue to a playlist.
        /// </summary>
        public async void SaveQueue()
        {
            var playlistName = await _dialogService.ShowAddToPlaylistDialog(false);
            if (playlistName == null) return;

            var req = await _mpdService.SafelySendCommandAsync(new SaveCommand(playlistName));

            if (req != null)
                _notificationService.ShowInAppNotification(string.Format(Resources.AddedToPlaylistText, playlistName));
        }

        /// <summary>
        /// Clear the MPD queue.
        /// </summary>
        public async void ClearQueue() => await _mpdService.SafelySendCommandAsync(new ClearCommand());

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
            shuffleTasks.Add(Task.Run(async () =>
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

            // Set the volume
            shuffleTasks.Add(Task.Run(async () =>
            {
                await _mpdService.SafelySendCommandAsync(new RandomCommand(IsShuffleEnabled));
                Thread.Sleep(1000); // Wait for MPD to acknowledge the new status...
                await _dispatcherService.ExecuteOnUIThreadAsync(async () => await UpdateUpNextAsync(_mpdService.CurrentStatus));
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

                _ = Task.Run(async () => {
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

        private async void OnStateChange(object sender, EventArgs eventArgs)
        {
            var status = _mpdService.CurrentStatus;

            await _dispatcherService.ExecuteOnUIThreadAsync(() =>
            {
                // Remove completed requests
                volumeTasks.RemoveAll(t => t.IsCompleted);
                shuffleTasks.RemoveAll(t => t.IsCompleted);

                // Update volume to match the server value -- If we're not setting it ourselves
                if (volumeTasks.Count == 0)
                {
                    _internalVolume = status.Volume;
                    OnPropertyChanged(nameof(MediaVolume));
                }

                // Ditto for shuffle/repeat/single
                if (shuffleTasks.Count == 0)
                {
                    _isShuffledEnabled = status.Random;
                    _isRepeatEnabled = status.Repeat;
                    _isSingleEnabled = status.Single;

                    if (_isSingleEnabled)
                        RepeatIcon = "\uE8ED";
                    else
                        RepeatIcon = "\uE8EE";

                    OnPropertyChanged(nameof(IsRepeatEnabled));
                    OnPropertyChanged(nameof(IsShuffleEnabled));
                }

                switch (status.State)
                {
                    case MpdState.Play:
                        IsTrackInfoAvailable = true;
                        PlayButtonContent = "\uE769";
                        break;

                    case MpdState.Stop:
                        IsTrackInfoAvailable = true;
                        PlayButtonContent = "\uE768";
                        break;

                    case MpdState.Pause:
                        IsTrackInfoAvailable = true;
                        PlayButtonContent = "\uE768";
                        break;

                    default:
                        IsTrackInfoAvailable = false;
                        PlayButtonContent = "\uE768";
                        break;
                }
            });
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

        private ICommand _addToPlaylistCommand;
        public ICommand AddToPlaylistCommand => _addToPlaylistCommand ?? (_addToPlaylistCommand = new RelayCommand<EventArgs>(AddToPlaylist));
        private void AddToPlaylist(EventArgs obj)
        {
            // Track must exist
            if (CurrentTrack == null)
            {
                _notificationService.ShowInAppNotification(Resources.NoTrackPlayingText);
            }

            CurrentTrack.AddToPlayListCommand.Execute(CurrentTrack.File);
        }

        private ICommand _showAlbumCommand;
        public ICommand ShowAlbumCommand => _showAlbumCommand ?? (_showAlbumCommand = new RelayCommand<EventArgs>(ShowAlbum));
        private void ShowAlbum(EventArgs obj)
        {
            // Track must exist
            if (CurrentTrack == null)
            {
                _notificationService.ShowInAppNotification(Resources.NoTrackPlayingText);
                return;
            }

            CurrentTrack.ViewAlbumCommand.Execute(CurrentTrack.File);
        }

        private ICommand _compactViewCommand;
        public ICommand SwitchToCompactViewCommand => _compactViewCommand ?? (_compactViewCommand = new AsyncRelayCommand<EventArgs>(SwitchToCompactViewAsync));

        /// <summary>
        ///     Switch to compact overlay mode
        /// </summary>
        public abstract Task SwitchToCompactViewAsync(EventArgs obj);
        

        #endregion Method Bindings
    }
}
