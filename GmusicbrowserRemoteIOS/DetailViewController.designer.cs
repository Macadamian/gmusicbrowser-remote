// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace GmusicbrowserRemoteIOS
{
	[Register ("DetailViewController")]
	partial class DetailViewController
	{
		[Outlet]
		MonoTouch.UIKit.UILabel detailDescriptionLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIToolbar toolbar { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel TitleLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel ArtistLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UISlider VolumeSlider { get; set; }

		[Outlet]
		MonoTouch.UIKit.UISlider SeekSlider { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton PlayPauseButton { get; set; }

		[Action ("SkipClicked:")]
		partial void SkipClicked (MonoTouch.Foundation.NSObject sender);

		[Action ("PlayPauseClicked:")]
		partial void PlayPauseClicked (MonoTouch.Foundation.NSObject sender);

		[Action ("PrevClicked:")]
		partial void PrevClicked (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (detailDescriptionLabel != null) {
				detailDescriptionLabel.Dispose ();
				detailDescriptionLabel = null;
			}

			if (toolbar != null) {
				toolbar.Dispose ();
				toolbar = null;
			}

			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}

			if (ArtistLabel != null) {
				ArtistLabel.Dispose ();
				ArtistLabel = null;
			}

			if (VolumeSlider != null) {
				VolumeSlider.Dispose ();
				VolumeSlider = null;
			}

			if (SeekSlider != null) {
				SeekSlider.Dispose ();
				SeekSlider = null;
			}

			if (PlayPauseButton != null) {
				PlayPauseButton.Dispose ();
				PlayPauseButton = null;
			}
		}
	}
}
