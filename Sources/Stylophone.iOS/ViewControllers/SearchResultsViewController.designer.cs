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
	[Register ("SearchResultsViewController")]
	partial class SearchResultsViewController
	{
		[Outlet]
		UIKit.UIView EmptyView { get; set; }

		[Outlet]
		UIKit.UIView SearchInProgressView { get; set; }

		[Outlet]
		UIKit.UISegmentedControl SearchSegmentedControl { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (EmptyView != null) {
				EmptyView.Dispose ();
				EmptyView = null;
			}

			if (SearchSegmentedControl != null) {
				SearchSegmentedControl.Dispose ();
				SearchSegmentedControl = null;
			}

			if (SearchInProgressView != null) {
				SearchInProgressView.Dispose ();
				SearchInProgressView = null;
			}
		}
	}
}
