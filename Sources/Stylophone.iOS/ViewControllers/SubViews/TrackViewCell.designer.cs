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
	[Register ("TrackViewCell")]
	partial class TrackViewCell
	{
		[Outlet]
		UIKit.UIButton AlbumTitle { get; set; }

		[Outlet]
		UIKit.UILabel Artist { get; set; }

		[Outlet]
		UIKit.UILabel Duration { get; set; }

		[Outlet]
		UIKit.UIImageView NowPlayingIndicator { get; set; }

		[Outlet]
		UIKit.UILabel Title { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (Title != null) {
				Title.Dispose ();
				Title = null;
			}

			if (Artist != null) {
				Artist.Dispose ();
				Artist = null;
			}

			if (AlbumTitle != null) {
				AlbumTitle.Dispose ();
				AlbumTitle = null;
			}

			if (Duration != null) {
				Duration.Dispose ();
				Duration = null;
			}

			if (NowPlayingIndicator != null) {
				NowPlayingIndicator.Dispose ();
				NowPlayingIndicator = null;
			}
		}
	}
}
