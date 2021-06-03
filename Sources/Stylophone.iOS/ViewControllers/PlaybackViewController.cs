// This file has been autogenerated from a class added in the UI designer.

using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using SkiaSharp;
using Stylophone.Common.Helpers;
using Stylophone.Common.ViewModels;
using Stylophone.iOS.Helpers;
using Stylophone.iOS.ViewModels;
using UIKit;

namespace Stylophone.iOS.ViewControllers
{
	public partial class PlaybackViewController : UIViewController, IViewController<PlaybackViewModel>
	{
		public PlaybackViewController (IntPtr handle) : base (handle)
		{
		}

        public PlaybackViewModel ViewModel { get; private set; }
		public PropertyBinder<PlaybackViewModel> Binder { get; private set; }
        private PropertyBinder<TrackViewModel> _trackBinder;

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;

            // PlaybackVM is transient, so we need to initialize it explicitly.
            ViewModel = Ioc.Default.GetRequiredService<PlaybackViewModel>();
            Binder = new(ViewModel);

            ViewModel.PropertyChanged += OnVmPropertyChanged;

            // Bind
            var negateBoolTransformer = NSValueTransformer.GetValueTransformer(nameof(ReverseBoolValueTransformer));

            // Compact View Binding
            Binder.Bind<bool>(CompactView, "hidden", nameof(ViewModel.IsTrackInfoAvailable), valueTransformer: negateBoolTransformer);

            CompactView.PrevButton.PrimaryActionTriggered += (s, e) => ViewModel.SkipPrevious();
            CompactView.NextButton.PrimaryActionTriggered += (s, e) => ViewModel.SkipNext();
            CompactView.PlayPauseButton.PrimaryActionTriggered += (s, e) => ViewModel.ChangePlaybackState();
            CompactView.ShuffleButton.PrimaryActionTriggered += (s, e) => ViewModel.ToggleShuffle();

            CompactView.OpenFullScreenButton.PrimaryActionTriggered += (s, e) => ViewModel.NavigateNowPlaying();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TrackSlider.TouchDragInside += (s, e) =>
            {
                ViewModel.TimeListened = Miscellaneous.FormatTimeString(TrackSlider.Value * 1000);
                ViewModel.OnPlayingSliderMoving();
            };
            TrackSlider.ValueChanged += (s, e) =>
            {
                ViewModel.CurrentTimeValue = TrackSlider.Value;
                ViewModel.OnPlayingSliderChange();
            };

            // Full View Binding
            Binder.Bind<string>(ElapsedTime, "text", nameof(ViewModel.TimeListened));
            Binder.Bind<string>(RemainingTime, "text", nameof(ViewModel.TimeRemaining));
            Binder.Bind<double>(TrackSlider, "value", nameof(ViewModel.CurrentTimeValue), true);
            Binder.Bind<double>(TrackSlider, "maximumValue", nameof(ViewModel.MaxTimeValue));
            UpdateFullView(ViewModel.CurrentTrack);

            SkipPrevButton.PrimaryActionTriggered += (s, e) => ViewModel.SkipPrevious();
            SkipNextButton.PrimaryActionTriggered += (s, e) => ViewModel.SkipNext();
            PlayPauseButton.PrimaryActionTriggered += (s, e) => ViewModel.ChangePlaybackState();
            //CompactView.ShuffleButton.PrimaryActionTriggered += (s, e) => ViewModel.ToggleShuffle();

            AlbumArt.Layer.CornerRadius = 8;
        }

        private void OnVmPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.CurrentTrack))
            {
                CompactView.Bind(ViewModel.CurrentTrack);
                UpdateFullView(ViewModel.CurrentTrack);
            }

            if (e.PropertyName == nameof(ViewModel.PlayButtonContent))
            {
                UpdateButton(CompactView.PlayPauseButton, ViewModel.PlayButtonContent);
                UpdateButton(PlayPauseButton, ViewModel.PlayButtonContent);
            }

            if (e.PropertyName == nameof(ViewModel.VolumeIcon))
            {
                UpdateButton(CompactView.VolumeButton, ViewModel.VolumeIcon);
            }

            if (e.PropertyName == nameof(ViewModel.IsShuffleEnabled))
            {
                UpdateButton(CompactView.ShuffleButton, ViewModel.IsShuffleEnabled ? "shuffle.circle.fill" : "shuffle.circle");
            }

            if (e.PropertyName == nameof(ViewModel.CurrentTimeValue))
            {
                var progress = (float)(ViewModel.CurrentTimeValue / ViewModel.MaxTimeValue);
                CompactView.CircularProgressView.Progress = progress * 100;
            }
        }

        private void UpdateFullView(TrackViewModel currentTrack)
        {
            // Don't bind if the view isn't loaded yet
            if (currentTrack == null || TrackTitle == null)
            {
                return;
            }

            // Bind trackData
            _trackBinder?.Dispose();
            _trackBinder = new PropertyBinder<TrackViewModel>(currentTrack);

            TrackTitle.Text = currentTrack.Name;
            ArtistName.Text = currentTrack.File?.Artist;
            AlbumName.Text = currentTrack.File?.Album;

            var imageConverter = NSValueTransformer.GetValueTransformer(nameof(SkiaToUIImageValueTransformer));
            var colorConverter = NSValueTransformer.GetValueTransformer(nameof(SkiaToUIColorValueTransformer));

            _trackBinder.Bind<SKImage>(AlbumArt, "image", nameof(currentTrack.AlbumArt), valueTransformer: imageConverter);
            _trackBinder.Bind<SKImage>(AlbumBackground, "image", nameof(currentTrack.AlbumArt), valueTransformer: imageConverter);
            _trackBinder.Bind<SKColor>(BackgroundTint, "backgroundColor", nameof(currentTrack.DominantColor), valueTransformer: colorConverter);
        }

        private void UpdateButton(UIButton button, string systemImg) =>
            button?.SetImage(UIImage.GetSystemImage(systemImg), UIControlState.Normal);
    }
}
