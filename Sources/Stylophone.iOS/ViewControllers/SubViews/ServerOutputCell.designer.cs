// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Stylophone.iOS.Views
{
	[Register ("ServerOutputCell")]
	partial class ServerOutputCell
	{
		[Outlet]
		UIKit.UILabel OutputLabel { get; set; }

		[Outlet]
		Stylophone.iOS.Helpers.KvoUISwitch OutputToggle { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (OutputLabel != null) {
				OutputLabel.Dispose ();
				OutputLabel = null;
			}

			if (OutputToggle != null) {
				OutputToggle.Dispose ();
				OutputToggle = null;
			}
		}
	}
}
