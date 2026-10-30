// WARNING
//
// This file has been generated automatically by Rider IDE
//   to store outlets and actions made in Xcode.
// If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Stylophone.iOS.ViewControllers
{
	[Register ("PlaybackViewController")]
	partial class PlaybackViewController
	{
		[Outlet]
		UIKit.UIImageView AlbumArt { get; set; }

		[Outlet]
		UIKit.UIImageView AlbumBackground { get; set; }

		[Outlet]
		UIKit.UILabel AlbumName { get; set; }

		[Outlet]
		UIKit.UILabel ArtistName { get; set; }

		[Outlet]
		UIKit.UIView BackgroundTint { get; set; }

		[Outlet]
		public Stylophone.iOS.ViewControllers.CompactPlaybackView CompactView { get; set; }

		[Outlet]
		UIKit.UILabel ElapsedTime { get; set; }

		[Outlet]
		UIKit.UIButton LocalMuteButton { get; set; }

		[Outlet]
		UIKit.UIView LocalPlaybackView { get; set; }

		[Outlet]
		UIKit.UILabel LocalVolume { get; set; }

		[Outlet]
		UIKit.UISlider LocalVolumeSlider { get; set; }

		[Outlet]
		UIKit.UIButton PlayPauseButton { get; set; }

		[Outlet]
		UIKit.UIView PlaybackContent { get; set; }

		[Outlet]
		UIKit.UILabel RemainingTime { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem RepeatButton { get; set; }

		[Outlet]
		UIKit.UIButton ServerMuteButton { get; set; }

		[Outlet]
		UIKit.UILabel ServerVolume { get; set; }

		[Outlet]
		UIKit.UISlider ServerVolumeSlider { get; set; }

		[Outlet]
		UIKit.UIView ShadowCaster { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem ShuffleButton { get; set; }

		[Outlet]
		UIKit.UIButton SkipNextButton { get; set; }

		[Outlet]
		UIKit.UIButton SkipPrevButton { get; set; }

		[Outlet]
		UIKit.UISlider TrackSlider { get; set; }

		[Outlet]
		UIKit.UILabel TrackTitle { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem VolumeButton { get; set; }

		[Outlet]
		UIKit.UIStackView VolumePopover { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (AlbumArt != null) {
				AlbumArt.Dispose ();
				AlbumArt = null;
			}

			if (AlbumBackground != null) {
				AlbumBackground.Dispose ();
				AlbumBackground = null;
			}

			if (AlbumName != null) {
				AlbumName.Dispose ();
				AlbumName = null;
			}

			if (ArtistName != null) {
				ArtistName.Dispose ();
				ArtistName = null;
			}

			if (BackgroundTint != null) {
				BackgroundTint.Dispose ();
				BackgroundTint = null;
			}

			if (CompactView != null) {
				CompactView.Dispose ();
				CompactView = null;
			}

			if (ElapsedTime != null) {
				ElapsedTime.Dispose ();
				ElapsedTime = null;
			}

			if (LocalMuteButton != null) {
				LocalMuteButton.Dispose ();
				LocalMuteButton = null;
			}

			if (LocalPlaybackView != null) {
				LocalPlaybackView.Dispose ();
				LocalPlaybackView = null;
			}

			if (LocalVolume != null) {
				LocalVolume.Dispose ();
				LocalVolume = null;
			}

			if (LocalVolumeSlider != null) {
				LocalVolumeSlider.Dispose ();
				LocalVolumeSlider = null;
			}

			if (PlayPauseButton != null) {
				PlayPauseButton.Dispose ();
				PlayPauseButton = null;
			}

			if (PlaybackContent != null) {
				PlaybackContent.Dispose ();
				PlaybackContent = null;
			}

			if (RemainingTime != null) {
				RemainingTime.Dispose ();
				RemainingTime = null;
			}

			if (ServerMuteButton != null) {
				ServerMuteButton.Dispose ();
				ServerMuteButton = null;
			}

			if (ServerVolume != null) {
				ServerVolume.Dispose ();
				ServerVolume = null;
			}

			if (ServerVolumeSlider != null) {
				ServerVolumeSlider.Dispose ();
				ServerVolumeSlider = null;
			}

			if (ShadowCaster != null) {
				ShadowCaster.Dispose ();
				ShadowCaster = null;
			}

			if (SkipNextButton != null) {
				SkipNextButton.Dispose ();
				SkipNextButton = null;
			}

			if (SkipPrevButton != null) {
				SkipPrevButton.Dispose ();
				SkipPrevButton = null;
			}

			if (TrackSlider != null) {
				TrackSlider.Dispose ();
				TrackSlider = null;
			}

			if (TrackTitle != null) {
				TrackTitle.Dispose ();
				TrackTitle = null;
			}

			if (VolumePopover != null) {
				VolumePopover.Dispose ();
				VolumePopover = null;
			}

			if (VolumeButton != null) {
				VolumeButton.Dispose ();
				VolumeButton = null;
			}

			if (ShuffleButton != null) {
				ShuffleButton.Dispose ();
				ShuffleButton = null;
			}

			if (RepeatButton != null) {
				RepeatButton.Dispose ();
				RepeatButton = null;
			}

		}
	}
}
