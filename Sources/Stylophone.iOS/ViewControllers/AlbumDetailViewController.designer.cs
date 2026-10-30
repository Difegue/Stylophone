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
	[Register ("AlbumDetailViewController")]
	partial class AlbumDetailViewController
	{
		[Outlet]
		UIKit.UIImageView AlbumArt { get; set; }

		[Outlet]
		UIKit.UILabel AlbumArtists { get; set; }

		[Outlet]
		UIKit.UILabel AlbumTitle { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView AlbumArtLoadingIndicator { get; set; }

		[Outlet]
		UIKit.UILabel AlbumTrackInfo { get; set; }

		[Outlet]
		UIKit.UIView ArtContainer { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint ArtWidthConstraint { get; set; }

		[Outlet]
		UIKit.UIImageView BackgroundArt { get; set; }

		[Outlet]
		UIKit.UIView EmptyView { get; set; }

		[Outlet]
		UIKit.UIVisualEffectView HeaderBlur { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (AlbumArt != null) {
				AlbumArt.Dispose ();
				AlbumArt = null;
			}

			if (AlbumArtists != null) {
				AlbumArtists.Dispose ();
				AlbumArtists = null;
			}

			if (AlbumTitle != null) {
				AlbumTitle.Dispose ();
				AlbumTitle = null;
			}

			if (AlbumArtLoadingIndicator != null) {
				AlbumArtLoadingIndicator.Dispose ();
				AlbumArtLoadingIndicator = null;
			}

			if (AlbumTrackInfo != null) {
				AlbumTrackInfo.Dispose ();
				AlbumTrackInfo = null;
			}

			if (ArtContainer != null) {
				ArtContainer.Dispose ();
				ArtContainer = null;
			}

			if (BackgroundArt != null) {
				BackgroundArt.Dispose ();
				BackgroundArt = null;
			}

			if (EmptyView != null) {
				EmptyView.Dispose ();
				EmptyView = null;
			}

			if (HeaderBlur != null) {
				HeaderBlur.Dispose ();
				HeaderBlur = null;
			}

			if (ArtWidthConstraint != null) {
				ArtWidthConstraint.Dispose ();
				ArtWidthConstraint = null;
			}
		}
	}
}
