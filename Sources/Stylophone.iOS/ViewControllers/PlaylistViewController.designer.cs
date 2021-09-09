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
	[Register ("PlaylistViewController")]
	partial class PlaylistViewController
	{
		[Outlet]
		UIKit.UIButton AddToQueueButton { get; set; }

		[Outlet]
		UIKit.UIImageView AlbumArt { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView AlbumArtLoadingIndicator { get; set; }

		[Outlet]
		UIKit.UIView ArtContainer { get; set; }

		[Outlet]
		UIKit.UIImageView BackgroundArt { get; set; }

		[Outlet]
		UIKit.UIButton DeleteButton { get; set; }

		[Outlet]
		UIKit.UIView EmptyView { get; set; }

		[Outlet]
		UIKit.UIButton PlayButton { get; set; }

		[Outlet]
		UIKit.UILabel PlaylistArtists { get; set; }

		[Outlet]
		UIKit.UILabel PlaylistInfo { get; set; }

		[Outlet]
		UIKit.UILabel PlaylistTitle { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (AddToQueueButton != null) {
				AddToQueueButton.Dispose ();
				AddToQueueButton = null;
			}

			if (AlbumArt != null) {
				AlbumArt.Dispose ();
				AlbumArt = null;
			}

			if (AlbumArtLoadingIndicator != null) {
				AlbumArtLoadingIndicator.Dispose ();
				AlbumArtLoadingIndicator = null;
			}

			if (ArtContainer != null) {
				ArtContainer.Dispose ();
				ArtContainer = null;
			}

			if (BackgroundArt != null) {
				BackgroundArt.Dispose ();
				BackgroundArt = null;
			}

			if (DeleteButton != null) {
				DeleteButton.Dispose ();
				DeleteButton = null;
			}

			if (PlayButton != null) {
				PlayButton.Dispose ();
				PlayButton = null;
			}

			if (EmptyView != null) {
				EmptyView.Dispose ();
				EmptyView = null;
			}

			if (PlaylistTitle != null) {
				PlaylistTitle.Dispose ();
				PlaylistTitle = null;
			}

			if (PlaylistInfo != null) {
				PlaylistInfo.Dispose ();
				PlaylistInfo = null;
			}

			if (PlaylistArtists != null) {
				PlaylistArtists.Dispose ();
				PlaylistArtists = null;
			}
		}
	}
}
