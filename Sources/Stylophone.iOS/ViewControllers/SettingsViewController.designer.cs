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
	[Register ("SettingsViewController")]
	partial class SettingsViewController
	{
		[Outlet]
		UIKit.UISwitch AlbumArtToggle { get; set; }

		[Outlet]
		UIKit.UISwitch AnalyticsToggle { get; set; }

		[Outlet]
		UIKit.UIButton ClearCacheButton { get; set; }

		[Outlet]
		UIKit.UIButton GithubButton { get; set; }

		[Outlet]
		UIKit.UISwitch LocalPlaybackToggle { get; set; }

		[Outlet]
		UIKit.UIButton RateButton { get; set; }

		[Outlet]
		UIKit.UIView ServerConnectedBox { get; set; }

		[Outlet]
		UIKit.UIImageView ServerConnectionFailed { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView ServerConnectionIndicator { get; set; }

		[Outlet]
		UIKit.UIImageView ServerConnectionSuccess { get; set; }

		[Outlet]
		UIKit.UITextField ServerHostnameField { get; set; }

		[Outlet]
		UIKit.UILabel ServerInfoLabel { get; set; }

		[Outlet]
		UIKit.UITextField ServerPasswordField { get; set; }

		[Outlet]
		UIKit.UITextField ServerPortField { get; set; }

		[Outlet]
		UIKit.UIButton UpdateDatabaseButton { get; set; }

		[Outlet]
		UIKit.UILabel VersionLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (AnalyticsToggle != null) {
				AnalyticsToggle.Dispose ();
				AnalyticsToggle = null;
			}

			if (ClearCacheButton != null) {
				ClearCacheButton.Dispose ();
				ClearCacheButton = null;
			}

			if (GithubButton != null) {
				GithubButton.Dispose ();
				GithubButton = null;
			}

			if (LocalPlaybackToggle != null) {
				LocalPlaybackToggle.Dispose ();
				LocalPlaybackToggle = null;
			}

			if (AlbumArtToggle != null) {
				AlbumArtToggle.Dispose ();
				AlbumArtToggle = null;
			}

			if (RateButton != null) {
				RateButton.Dispose ();
				RateButton = null;
			}

			if (ServerConnectedBox != null) {
				ServerConnectedBox.Dispose ();
				ServerConnectedBox = null;
			}

			if (ServerConnectionFailed != null) {
				ServerConnectionFailed.Dispose ();
				ServerConnectionFailed = null;
			}

			if (ServerConnectionIndicator != null) {
				ServerConnectionIndicator.Dispose ();
				ServerConnectionIndicator = null;
			}

			if (ServerConnectionSuccess != null) {
				ServerConnectionSuccess.Dispose ();
				ServerConnectionSuccess = null;
			}

			if (ServerHostnameField != null) {
				ServerHostnameField.Dispose ();
				ServerHostnameField = null;
			}

			if (ServerInfoLabel != null) {
				ServerInfoLabel.Dispose ();
				ServerInfoLabel = null;
			}

			if (ServerPasswordField != null) {
				ServerPasswordField.Dispose ();
				ServerPasswordField = null;
			}

			if (ServerPortField != null) {
				ServerPortField.Dispose ();
				ServerPortField = null;
			}

			if (UpdateDatabaseButton != null) {
				UpdateDatabaseButton.Dispose ();
				UpdateDatabaseButton = null;
			}

			if (VersionLabel != null) {
				VersionLabel.Dispose ();
				VersionLabel = null;
			}
		}
	}
}
