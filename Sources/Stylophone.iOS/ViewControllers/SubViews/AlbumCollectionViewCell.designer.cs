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
	[Register ("AlbumCollectionViewCell")]
	partial class AlbumCollectionViewCell
	{
		[Outlet]
		UIKit.UIImageView AlbumArt { get; set; }

		[Outlet]
		UIKit.UILabel AlbumArtist { get; set; }

		[Outlet]
		UIKit.UIVisualEffectView AlbumInfoView { get; set; }

		[Outlet]
		UIKit.UILabel AlbumName { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView ArtLoadingIndicator { get; set; }

		[Outlet]
		UIKit.UIView CornerRadiusContainer { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (AlbumArt != null) {
				AlbumArt.Dispose ();
				AlbumArt = null;
			}

			if (AlbumArtist != null) {
				AlbumArtist.Dispose ();
				AlbumArtist = null;
			}

			if (AlbumInfoView != null) {
				AlbumInfoView.Dispose ();
				AlbumInfoView = null;
			}

			if (AlbumName != null) {
				AlbumName.Dispose ();
				AlbumName = null;
			}

			if (ArtLoadingIndicator != null) {
				ArtLoadingIndicator.Dispose ();
				ArtLoadingIndicator = null;
			}

			if (CornerRadiusContainer != null) {
				CornerRadiusContainer.Dispose ();
				CornerRadiusContainer = null;
			}
		}
	}
}
