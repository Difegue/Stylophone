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
		UIKit.UILabel TrackTitle { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CompactView != null) {
				CompactView.Dispose ();
				CompactView = null;
			}

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

			if (TrackTitle != null) {
				TrackTitle.Dispose ();
				TrackTitle = null;
			}
		}
	}
}
