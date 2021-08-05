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
	[Register ("FoldersViewController")]
	partial class FoldersViewController
	{
		[Outlet]
		UIKit.UIActivityIndicatorView LoadingIndicator { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LoadingIndicator != null) {
				LoadingIndicator.Dispose ();
				LoadingIndicator = null;
			}
		}
	}
}
