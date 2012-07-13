// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace GmusicbrowserRemoteIOS
{
	[Register ("RootViewController")]
	partial class RootViewController
	{
		[Outlet]
		MonoTouch.UIKit.UITableView ServerList { get; set; }

		[Action ("AddButtonClicked:")]
		partial void AddButtonClicked (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (ServerList != null) {
				ServerList.Dispose ();
				ServerList = null;
			}
		}
	}
}
