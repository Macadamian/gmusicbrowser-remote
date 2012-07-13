using System;
using System.Drawing;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using GmusicbrowserRemote;
using GmusicbrowserRemote.Core;
using System.Threading.Tasks;

namespace GmusicbrowserRemoteIOS
{
    public partial class DetailViewController : UIViewController
    {
        Gmusicbrowser gmb;

        static bool UserInterfaceIdiomIsPhone {
            get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
        }

        UIPopoverController popoverController;
		
        public DetailViewController (IntPtr handle) : base (handle) {
        }
		
        void ConfigureView () {
            // Update the user interface for the detail item
            if (gmb != null && this.detailDescriptionLabel != null) {
                this.NavigationItem.Title = gmb.Hostname;
                this.detailDescriptionLabel.Text = gmb.Hostname;
            }
        }
		
        public override void DidReceiveMemoryWarning () {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning ();
			
            // Release any cached data, images, etc that aren't in use.
        }
		
		#region View lifecycle
		
        public override void ViewDidLoad () {
            base.ViewDidLoad ();
			
            // Perform any additional setup after loading the view, typically from a nib.
            ConfigureView ();
			
            if (!UserInterfaceIdiomIsPhone)
                SplitViewController.Delegate = new SplitViewControllerDelegate ();
        }
		
        public override void ViewDidUnload () {
            base.ViewDidUnload ();
			
            // Release any retained subviews of the main view.
        }
		
        public override void ViewWillAppear (bool animated) {
            base.ViewWillAppear (animated);
        }
		
        public override void ViewDidAppear (bool animated) {
            base.ViewDidAppear (animated);
        }
		
        public override void ViewWillDisappear (bool animated) {
            base.ViewWillDisappear (animated);
        }
		
        public override void ViewDidDisappear (bool animated) {
            base.ViewDidDisappear (animated);
        }
		
		#endregion
		
        public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation) {
            // Return true for supported orientations
            if (UserInterfaceIdiomIsPhone) {
                return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
            } else {
                return true;
            }
        }
		
		#region Split View
		
        class SplitViewControllerDelegate : UISplitViewControllerDelegate
        {
            public override void WillHideViewController (UISplitViewController svc, UIViewController aViewController, UIBarButtonItem barButtonItem, UIPopoverController pc) {
                var dv = svc.ViewControllers [1] as DetailViewController;
                barButtonItem.Title = "Player List";
                var items = new List<UIBarButtonItem> ();
                items.Add (barButtonItem);
                items.AddRange (dv.toolbar.Items);
                dv.toolbar.SetItems (items.ToArray (), true);
                dv.popoverController = pc;
            }
			
            public override void WillShowViewController (UISplitViewController svc, UIViewController aViewController, UIBarButtonItem button) {
                var dv = svc.ViewControllers [1] as DetailViewController;
                var items = new List<UIBarButtonItem> (dv.toolbar.Items);
                items.RemoveAt (0);
                dv.toolbar.SetItems (items.ToArray (), true);
                dv.popoverController = null;
            }
        }
		
		#endregion

        public void HandleUpdatedStateFromNetwork (Player state) {
            this.InvokeOnMainThread(() => {
                this.VolumeSlider.SetValue(state.Volume.Value, true);
                if(state.Current != null) {
                    this.ArtistLabel.Text = state.Current.Artist;
                    this.TitleLabel.Text = state.Current.Title;
                    this.SeekSlider.MaxValue = state.Current.Length.Value;
                    if(state.PlayPosition.HasValue) {
                        this.SeekSlider.SetValue (state.PlayPosition.Value, true);
                    } else {
                        this.SeekSlider.Value = 0;
                    }
                } else {
                    this.ArtistLabel.Text = "";
                    this.TitleLabel.Text = "";
                    this.SeekSlider.SetValue (0, false);
                }
                if(state.Playing == 1) {
                    this.PlayPauseButton.TitleLabel.Text = "Pause";
                } else {
                    this.PlayPauseButton.TitleLabel.Text = "Play";
                }
            });
        }

        public void HandleUpdatedStateFromNetworkCompletedTask (Task<Player> task) {
            if (task.IsCanceled || task.IsFaulted) {
            } else {
                HandleUpdatedStateFromNetwork (task.Result);
            }
        }

        public void UpdateFromServer() {
            this.gmb.FetchCurrentPlayerState().ContinueWith(HandleUpdatedStateFromNetworkCompletedTask);
        }

        public void SetGmusicbrowser(Gmusicbrowser gmb) {
            // this.detailDescriptionLabel.Text = gmb.Hostname;

            if (this.gmb != gmb) {
                this.gmb = gmb;
                
                // Update the view
                ConfigureView ();
                UpdateFromServer ();
            }
            
            if (this.popoverController != null)
                this.popoverController.Dismiss (true);
        }

        partial void SkipClicked(NSObject sender) {
            this.gmb.Next().ContinueWith(HandleUpdatedStateFromNetworkCompletedTask);
        }

        partial void PrevClicked(NSObject sender) {
            this.gmb.Previous().ContinueWith(HandleUpdatedStateFromNetworkCompletedTask);
        }
    }
}

