using FluentMPC.Helpers;
using FluentMPC.Services;
using FluentMPC.ViewModels.Items;
using FluentMPC.Views;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Commands.Queue;
using MpcNET.Commands.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentMPC.ViewModels.Playback
{
    public class PlaybackViewModel : Observable
    {
        private readonly CoreDispatcher _currentUiDispatcher;
        private readonly int _artWidth;

        #region Getters and Setters

        public bool HideTrackName => NavigationService.Frame.CurrentSourcePageType != typeof(PlaybackView);

        /// <summary>
        /// The current playing track
        /// </summary>
        public TrackViewModel CurrentTrack
        {
            get => _currentTrack;
            set
            {
                Set(ref _currentTrack, value);
            }
        }

        private TrackViewModel _currentTrack;

        /// <summary>
        /// The next track in queue
        /// </summary>
        public TrackViewModel NextTrack
        {
            get => _nextTrack;
            set
            {
                Set(ref _nextTrack, value);
                OnPropertyChanged(nameof(HasNextTrack));
            }
        }

        private TrackViewModel _nextTrack;

        public bool HasNextTrack => NextTrack != null;

        /// <summary>
        ///     The amount of time spent listening to the track
        /// </summary>
        public string TimeListened
        {
            get => _timeListened;
            set
            {
                Set(ref _timeListened, value);
            }
        }

        private string _timeListened = "00:00";

        /// <summary>
        ///     The amount of time remaining
        /// </summary>
        public string TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                Set(ref _timeRemaining, value);
            }
        }

        private string _timeRemaining = "-00:00";

        /// <summary>
        ///     The current slider value
        /// </summary>
        public double CurrentTimeValue
        {
            get => _currentTimeValue;
            set
            {
                Set(ref _currentTimeValue, value);
            }
        }

        private double _currentTimeValue;

        /// <summary>
        ///     The max slider value
        /// </summary>
        public double MaxTimeValue
        {
            get => _maxTimeValue;
            private set
            {
                Set(ref _maxTimeValue, value);
            }
        }

        private double _maxTimeValue = 100;

        /// <summary>
        ///     The current text for the volume icon
        /// </summary>
        public string VolumeIcon
        {
            get => _volumeIcon;
            private set
            {
                Set(ref _volumeIcon, value);
            }
        }

        private string _volumeIcon = "\uE767";


        /// <summary>
        ///     The current text for the repeat icon
        /// </summary>
        public string RepeatIcon
        {
            get => _repeatIcon;
            private set
            {
                Set(ref _repeatIcon, value);
            }
        }

        private string _repeatIcon = "\uE8EE";

        /// <summary>
        ///     The content on the play_pause button
        /// </summary>
        public string PlayButtonContent
        {
            get => _playButtonContent;
            set
            {
                Set(ref _playButtonContent, value);
            }
        }

        private string _playButtonContent = "\uE769";

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
                    await MPDConnectionService.SafelySendCommandAsync(new SetVolumeCommand((byte)value), _currentUiDispatcher);
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
        private CancellationTokenSource cts = new CancellationTokenSource();
        private List<Task> volumeTasks = new List<Task>();
        private List<Task> shuffleTasks = new List<Task>();
        private double _internalVolume = MPDConnectionService.CurrentStatus.Volume;

        /// <summary>
        ///     Are tracks shuffled
        /// </summary>
        public bool IsShuffleEnabled
        {
            get => _isShuffledEnabled;
            set
            {
                Set(ref _isShuffledEnabled, value);
            }
        }

        private bool _isShuffledEnabled;

        /// <summary>
        ///     Is the song going to repeat when finished
        /// </summary>
        public bool IsRepeatEnabled
        {
            get => _isRepeatEnabled;
            set
            {
                Set(ref _isRepeatEnabled, value);
            }
        }

        private bool _isRepeatEnabled;

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

        private bool _isSingleEnabled;

        #endregion Getters and Setters

        #region Timers

        /// <summary>
        ///     This timer runs every 500ms to ensure that the current position,
        ///     time, remaining time, etc. variables are correct.
        /// </summary>
        private readonly DispatcherTimer _updateInformationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };

        #endregion Timers

        #region Constructors

        public PlaybackViewModel() : this(CoreApplication.MainView.Dispatcher)
        { }

        public PlaybackViewModel(CoreDispatcher uiDispatcher, int albumArtWidth = -1)
        {
            _currentUiDispatcher = uiDispatcher;
            _artWidth = albumArtWidth;

            // Bind the methods that we need
            MPDConnectionService.StatusChanged += OnStateChange;
            MPDConnectionService.SongChanged += OnTrackChange;

            // Bind timer methods
            _updateInformationTimer.Tick += UpdateInformation;

            // Update info to current track
            MPDConnectionService.ConnectionChanged += OnConnectionChanged;
            OnConnectionChanged(null, null);

            Application.Current.LeavingBackground += CurrentOnLeavingBackground;
            NavigationService.Navigated += (s, e) => DispatcherHelper.AwaitableRunAsync(_currentUiDispatcher, () => OnPropertyChanged(nameof(HideTrackName)));

            // Start the timer if ready
            if (!_updateInformationTimer.IsEnabled)
                _updateInformationTimer.Start();
        }

        private void OnConnectionChanged(object sender, EventArgs e)
        {
            if (MPDConnectionService.IsConnected)
            {
                OnTrackChange(this, new SongChangedEventArgs { NewSongId = MPDConnectionService.CurrentStatus.SongId });
                CurrentTimeValue = MPDConnectionService.CurrentStatus.Elapsed.TotalSeconds;

                OnStateChange(this, null);
                UpdateUpNextAsync();
            }
        }

        private bool _isOnMainDispatcher => _currentUiDispatcher == CoreApplication.MainView.CoreWindow.Dispatcher;

        private void CurrentOnLeavingBackground(object sender, LeavingBackgroundEventArgs leavingBackgroundEventArgs)
        {
            // Refresh all
            UpdateInformation(this, null);
        }

        #endregion Constructors

        #region Timer Methods

        /// <summary>
        ///     Timer method that is run to make sure the UI is kept up to date
        /// </summary>
        private async void UpdateInformation(object sender, object e)
        {
            // Only call the following if the player exists and the time is greater then 0.
            if (MPDConnectionService.CurrentStatus.Elapsed.TotalMilliseconds <= 0)
                return;

            if (CurrentTrack == null)
                return;

            await _currentUiDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Set the current time value - if the user isn't scrobbling the slider
                if (!_isUserMovingSlider)
                    CurrentTimeValue = MPDConnectionService.CurrentStatus.Elapsed.TotalSeconds;

                // Get the remaining time for the track
                var remainingTime = MPDConnectionService.CurrentStatus.Duration.Subtract(MPDConnectionService.CurrentStatus.Elapsed);

                // Set the time listened text
                TimeListened = MiscHelpers.FormatTimeString(MPDConnectionService.CurrentStatus.Elapsed.TotalMilliseconds);

                // Set the time remaining text
                TimeRemaining = "-" + MiscHelpers.FormatTimeString(remainingTime.TotalMilliseconds);

                // Set the maximum value
                MaxTimeValue = MPDConnectionService.CurrentStatus.Duration.TotalSeconds;
            });
        }

        #endregion Timer Methods

        #region Track Control Methods

        /// <summary>
        /// Write the current queue to a playlist.
        /// </summary>
        public async void SaveQueue()
        {
            var playlistName = await DialogService.ShowAddToPlaylistDialog(false);
            if (playlistName == null) return;

            // Adding a file to a playlist somehow triggers the server's "playlist" event, which is normally used for the queue...
            // We disable queue events temporarily in order to avoid UI jitter by a refreshed queue.
            MPDConnectionService.DisableQueueEvents = true;

            var req = await MPDConnectionService.SafelySendCommandAsync(new SaveCommand(playlistName), _currentUiDispatcher);

            if (req != null)
                NotificationService.ShowInAppNotification(string.Format("AddedToPlaylistText".GetLocalized(), playlistName));

            MPDConnectionService.DisableQueueEvents = false;
        }

        /// <summary>
        /// Clear the MPD queue.
        /// </summary>
        public async void ClearQueue() => await MPDConnectionService.SafelySendCommandAsync(new ClearCommand(), _currentUiDispatcher);

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
                await MPDConnectionService.SafelySendCommandAsync(new RepeatCommand(IsRepeatEnabled), _currentUiDispatcher);
                await MPDConnectionService.SafelySendCommandAsync(new SingleCommand(IsSingleEnabled), _currentUiDispatcher);
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
                await MPDConnectionService.SafelySendCommandAsync(new RandomCommand(IsShuffleEnabled), _currentUiDispatcher);

                await DispatcherHelper.AwaitableRunAsync(_currentUiDispatcher, UpdateUpNextAsync);
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
            NavigationService.Navigate(typeof(PlaybackView));
        }

        #endregion Track Control Methods

        #region Track Playback State

        /// <summary>
        ///     Toggles the state between the track playing
        ///     and not playing
        /// </summary>
        public async void ChangePlaybackState() => await MPDConnectionService.SafelySendCommandAsync(new PauseResumeCommand(), _currentUiDispatcher);

        /// <summary>
        ///     Go forward one track
        /// </summary>
        public async void SkipNext() => await MPDConnectionService.SafelySendCommandAsync(new NextCommand(), _currentUiDispatcher);

        /// <summary>
        ///     Go backwards one track
        /// </summary>
        public async void SkipPrevious() => await MPDConnectionService.SafelySendCommandAsync(new PreviousCommand(), _currentUiDispatcher);

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
            var remainingTime = MPDConnectionService.CurrentStatus.Duration.Subtract(TimeSpan.FromSeconds(CurrentTimeValue));
            TimeListened = MiscHelpers.FormatTimeString(CurrentTimeValue * 1000);
            TimeRemaining = "-" + MiscHelpers.FormatTimeString(remainingTime.TotalMilliseconds);

            // Set the track position
            await MPDConnectionService.SafelySendCommandAsync(new SeekCurCommand(CurrentTimeValue), _currentUiDispatcher);

            // Wait for MPD Status to catch up before we start auto-updating the slider again
            _ = Task.Run(() =>
              {
                  Thread.Sleep(1000);
                  _isUserMovingSlider = false;
              });
        }
        private async void UpdateUpNextAsync()
        {
            var nextSongId = MPDConnectionService.CurrentStatus.NextSongId;
            if (nextSongId != -1)
            {
                var response = await MPDConnectionService.SafelySendCommandAsync(new PlaylistIdCommand(nextSongId), _currentUiDispatcher);

                if (response != null)
                {
                    NextTrack = new TrackViewModel(response.First(), false, -1, _currentUiDispatcher);
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
            var response = await MPDConnectionService.SafelySendCommandAsync(new CurrentSongCommand(), _currentUiDispatcher);

            // Run all this on the UI thread
            await _currentUiDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (response != null)
                {
                    // Set the new current track, updating the UI with the correct Dispatcher
                    CurrentTrack = new TrackViewModel(response, true, _artWidth, _currentUiDispatcher);
                }
                else
                {
                    // TODO, No response 
                }

                if (CurrentTrack?.File != null)
                {
                    TimeRemaining = "-" + MiscHelpers.FormatTimeString(CurrentTrack.File.Time / 1000);
                    TimeListened = "00:00";
                    CurrentTimeValue = 0;
                    MaxTimeValue = CurrentTrack.File.Time;

                    Task.Run(() =>
                    {
                        Singleton<LiveTileService>.Instance.UpdatePlayingSong(CurrentTrack);
                        SystemMediaControlsService.UpdateMetadata(CurrentTrack);
                    });
                    
                    UpdateUpNextAsync();

                }
                else
                {
                    // TODO no track playing
                }
            });

        }

        private async void OnStateChange(object sender, EventArgs eventArgs)
        {
            await _currentUiDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // Remove completed requests
                volumeTasks.RemoveAll(t => t.IsCompleted);
                shuffleTasks.RemoveAll(t => t.IsCompleted);

                // Update volume to match the server value -- If we're not setting it ourselves
                if (volumeTasks.Count == 0)
                {
                    _internalVolume = MPDConnectionService.CurrentStatus.Volume;
                    OnPropertyChanged(nameof(MediaVolume));
                }

                // Ditto for shuffle/repeat/single
                if (shuffleTasks.Count == 0)
                {
                    _isShuffledEnabled = MPDConnectionService.CurrentStatus.Random;
                    _isRepeatEnabled = MPDConnectionService.CurrentStatus.Repeat;
                    _isSingleEnabled = MPDConnectionService.CurrentStatus.Single;

                    if (_isSingleEnabled)
                        RepeatIcon = "\uE8ED";
                    else
                        RepeatIcon = "\uE8EE";

                    OnPropertyChanged(nameof(IsRepeatEnabled));
                    OnPropertyChanged(nameof(IsShuffleEnabled));
                }

                switch (MPDConnectionService.CurrentStatus.State)
                {
                    case MpdState.Play:
                        PlayButtonContent = "\uE769";
                        break;

                    case MpdState.Stop:
                        PlayButtonContent = "\uE768";
                        break;

                    case MpdState.Pause:
                        PlayButtonContent = "\uE768";
                        break;

                    default:
                        PlayButtonContent = "\uE768";
                        break;
                }
            });
        }

        #endregion Methods

        public void Dispose()
        {
            // Unbind the methods that we need
            MPDConnectionService.StatusChanged -= OnStateChange;
            MPDConnectionService.SongChanged -= OnTrackChange;

            // Unbind timer methods
            _updateInformationTimer.Tick -= UpdateInformation;

            Application.Current.LeavingBackground -= CurrentOnLeavingBackground;
        }

        #region Commands

        private ICommand _addToPlaylistCommand;
        public ICommand AddToPlaylistCommand => _addToPlaylistCommand ?? (_addToPlaylistCommand = new RelayCommand<SelectionChangedEventArgs>(AddToPlaylist));
        private void AddToPlaylist(SelectionChangedEventArgs obj)
        {
            // Track must exist
            if (CurrentTrack == null)
            {
                if (_isOnMainDispatcher)
                    NotificationService.ShowInAppNotification("NoTrackPlayingText".GetLocalized());
                return;
            }

            CurrentTrack.AddToPlayListCommand.Execute(CurrentTrack.File);
        }

        private ICommand _showAlbumCommand;
        public ICommand ShowAlbumCommand => _showAlbumCommand ?? (_showAlbumCommand = new RelayCommand<SelectionChangedEventArgs>(ShowAlbum));
        private void ShowAlbum(SelectionChangedEventArgs obj)
        {
            // Track must exist
            if (CurrentTrack == null)
            {
                if (_isOnMainDispatcher)
                    NotificationService.ShowInAppNotification("NoTrackPlayingText".GetLocalized());
                return;
            }

            CurrentTrack.ViewAlbumCommand.Execute(CurrentTrack.File);
        }

        /// <summary>
        ///     Switch to compact overlay mode
        /// </summary>
        public async void SwitchToCompactView()
        {
            try
            {
                var compactView = CoreApplication.CreateNewView();
                var compactViewId = -1;
                var currentViewId = -1;

                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    // Get the Id back
                    currentViewId = ApplicationView.GetForCurrentView().Id;
                });

                // Create a new window within the view
                await DispatcherHelper.ExecuteOnUIThreadAsync(compactView, () =>
                {
                    // Create a new frame and navigate it to the overlay view
                    var overlayFrame = new Frame();
                    overlayFrame.Navigate(typeof(OverlayView), currentViewId);

                    // Set the window content and activate it
                    Window.Current.Content = overlayFrame;
                    Window.Current.Activate();

                    // Get the Id back
                    compactViewId = ApplicationView.GetForCurrentView().Id;
                });

                // Make the overlay small
                var compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
                compactOptions.CustomSize = new Size(300, 430);

                // Display as compact overlay
                await ApplicationViewSwitcher.TryShowAsViewModeAsync(compactViewId, ApplicationViewMode.CompactOverlay,
                    compactOptions);

                // Switch to this window
                await ApplicationViewSwitcher.SwitchAsync(compactViewId, currentViewId,
                    ApplicationViewSwitchingOptions.ConsolidateViews);
            }
            catch (Exception e)
            {
                NotificationService.ShowInAppNotification(string.Format("GenericErrorText".GetLocalized(), e), 0);
            }
        }

        #endregion Method Bindings
    }
}
