using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using GmusicbrowserRemote;
using System.IO.IsolatedStorage;
using MonoTouch.Dialog;
using GmusicbrowserRemote.Core;
using System.Collections.Generic;

namespace GmusicbrowserRemoteIOS
{
    public partial class RootViewController : UITableViewController
    {
        private delegate void ServerListTableSourceReady();

        private class ServerListTableSource : UITableViewSource {
            RootViewController rvc;
            UITableView table;
            ServerList serverList;
            List<Gmusicbrowser> currentServers;

            public ServerListTableSource(RootViewController rvc, UITableView table, ServerListTableSourceReady readyCb) {
                this.rvc = rvc;
                this.table = table;
                this.serverList = new ServerList();
                this.serverList.LoadFromStorage().ContinueWith((storageResult) => {
                    if(storageResult.IsCanceled || storageResult.IsFaulted) {
                        Console.Out.WriteLine ("AUGH FAILURE: " + storageResult.Exception);
                    } else {
                        this.BeginInvokeOnMainThread(() => {
                            this.currentServers = storageResult.Result;
                            table.ReloadData();
                            readyCb();
                        });
                    }
                });
            }

            #region implemented abstract members of MonoTouch.UIKit.UITableViewSource
            public override int RowsInSection (UITableView tableview, int section) {
                if (currentServers == null) {
                    return 0;
                } else {
                    return currentServers.Count;
                }
            }

            // http://monotouch.2284126.n4.nabble.com/Monotouch-Dialog-controller-calling-Storyboard-controller-a-bad-idea-td4528682.html
            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                var gmb = currentServers[indexPath.Row];
                var tc = new UITableViewCell();
                tc.TextLabel.Text = gmb.Hostname + ":" + gmb.Port;
                return tc;
            }

            public override int NumberOfSections (UITableView tableView) {
                return 1;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath path) {
                var remote = (DetailViewController) rvc.Storyboard.InstantiateViewController("remote_control");
                // var gmb = new Gmusicbrowser("macdesktop.orospakr.ca", 8081);
                var gmb = currentServers[path.Row];
                remote.SetGmusicbrowser(gmb);
                rvc.NavigationController.PushViewController(remote, true);
            }

            public void InsertNewServer (Gmusicbrowser newServer) {
                if (currentServers == null) {
                    return;
                }
                var newPos = currentServers.Count;
                currentServers.Add (newServer);
                var path = NSIndexPath.FromRowSection(newPos, 0);
                this.table.InsertRows(new NSIndexPath[] { path }, UITableViewRowAnimation.Fade);
                this.serverList.WriteToStorage(this.currentServers); // we'll just let that go off async, best-effort basis
            }
            #endregion
        }

        protected UIBarButtonItem addBarButton;

        ServerListTableSource serverListTableSource;

        static bool UserInterfaceIdiomIsPhone {
            get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
        }

        public RootViewController (IntPtr handle) : base (handle) {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {
                this.ClearsSelectionOnViewWillAppear = false;
                this.ContentSizeForViewInPopover = new SizeF (320f, 600f);
            }
			
            // Custom initialization
        }
		
        public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation) {
            // Return true for supported orientations
            if (UserInterfaceIdiomIsPhone) {
                return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
            } else {
                return true;
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
            addBarButton = new UIBarButtonItem(UIBarButtonSystemItem.Add);
            addBarButton.Enabled = false;

            this.NavigationItem.RightBarButtonItem = addBarButton;

            serverListTableSource = new ServerListTableSource(this, this.ServerList, () => {
                addBarButton.Enabled = true;
            });

            this.ServerList.Source = serverListTableSource;

            var hostEdit = new EntryElement("Hostname", "mygmb.local", "");
            var portEdit = new EntryElement("Port", "8081", "8081");

            addBarButton.Clicked += (sender, e) => {
                var gmbSettings = new Section() {
                    hostEdit, portEdit
                };
                var root = new RootElement("GMB Coordinates") { 
                    gmbSettings
                };

                var settingsDialogVC = new DialogViewController(root);
                var backButton =  new UIBarButtonItem(UIBarButtonSystemItem.Done, (s, args) => {
                    this.NavigationController.PopViewControllerAnimated(true);
                    var newGmb = new Gmusicbrowser(hostEdit.Value, Int16.Parse (portEdit.Value));
                    serverListTableSource.InsertNewServer(newGmb);
                });
               
                settingsDialogVC.NavigationItem.RightBarButtonItem = backButton;
                this.NavigationController.PushViewController(settingsDialogVC, true);
            };

            // Perform any additional setup after loading the view, typically from a nib.

        }
		
        public override void ViewDidUnload () {
            base.ViewDidUnload ();
			
            // Clear any references to subviews of the main view in order to
            // allow the Garbage Collector to collect them sooner.
            //
            // e.g. myOutlet.Dispose (); myOutlet = null;
			
            ReleaseDesignerOutlets ();
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
    }
}

