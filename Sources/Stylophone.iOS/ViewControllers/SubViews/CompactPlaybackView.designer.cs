// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Stylophone.iOS.ViewControllers
{
	[Register ("CompactPlaybackView")]
	partial class CompactPlaybackView
	{
		[Outlet]
		public UIKit.UIImageView AlbumArt { get; private set; }

		[Outlet]
		public UIKit.UIImageView AlbumBackground { get; private set; }

		[Outlet]
		UIKit.UILabel ArtistName { get; set; }

		[Outlet]
		public UIKit.UIView BackgroundTint { get; private set; }

		[Outlet]
		public Stylophone.iOS.Helpers.UICircularProgressView CircularProgressView { get; private set; }

		[Outlet]
		UIKit.UIView CornerRadiusContainer { get; set; }

		[Outlet]
		public UIKit.UIButton NextButton { get; private set; }

		[Outlet]
		public UIKit.UIButton OpenFullScreenButton { get; set; }

		[Outlet]
		public UIKit.UIButton PlayPauseButton { get; private set; }

		[Outlet]
		public UIKit.UIButton PrevButton { get; private set; }

		[Outlet]
		public UIKit.UIButton ShuffleButton { get; private set; }

		[Outlet]
		UIKit.UILabel TrackTitle { get; set; }

		[Outlet]
		public UIKit.UIButton VolumeButton { get; private set; }
		
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

			if (ArtistName != null) {
				ArtistName.Dispose ();
				ArtistName = null;
			}

			if (BackgroundTint != null) {
				BackgroundTint.Dispose ();
				BackgroundTint = null;
			}

			if (CircularProgressView != null) {
				CircularProgressView.Dispose ();
				CircularProgressView = null;
			}

			if (CornerRadiusContainer != null) {
				CornerRadiusContainer.Dispose ();
				CornerRadiusContainer = null;
			}

			if (NextButton != null) {
				NextButton.Dispose ();
				NextButton = null;
			}

			if (PlayPauseButton != null) {
				PlayPauseButton.Dispose ();
				PlayPauseButton = null;
			}

			if (PrevButton != null) {
				PrevButton.Dispose ();
				PrevButton = null;
			}

			if (ShuffleButton != null) {
				ShuffleButton.Dispose ();
				ShuffleButton = null;
			}

			if (TrackTitle != null) {
				TrackTitle.Dispose ();
				TrackTitle = null;
			}

			if (VolumeButton != null) {
				VolumeButton.Dispose ();
				VolumeButton = null;
			}

			if (OpenFullScreenButton != null) {
				OpenFullScreenButton.Dispose ();
				OpenFullScreenButton = null;
			}
		}
	}
}
