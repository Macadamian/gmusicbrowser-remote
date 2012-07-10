using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;

namespace GmusicbrowserRemote
{
    [Activity (Label = "GmusicbrowserRemote", MainLauncher = true)]
    public class Activity1 : Activity
    {
        readonly string c = "RemoteActivity";

        Gmusicbrowser gmb;

        TextView titleTextView;

        ImageButton playButton;

        TextView artistTextView;

        RatingBar ratingBar;

        ImageButton nextButton;

        ImageButton prevButton;

        SeekBar volumeSeekBar;

        SeekBar songSeekBar;

        protected void HandleUpdatedState (Player state) {
            // ANDREW: start here and figure out how to make the deserializer tweakable, OR switch to JSON.net
            Log.WriteLine(Android.Util.LogPriority.Info, "Activity1", String.Format ("SUCCESS, CURRENTLY {0}: {1} " , state.Playing ==  1 ? "Playing" : "Stopped", state.Current.Title));
            RunOnUiThread(() => {
                titleTextView.SetText (state.Current.Title, TextView.BufferType.Normal);
                artistTextView.SetText (state.Current.Artist, TextView.BufferType.Normal);
                if (state.Current.Rating.HasValue) {
                    ratingBar.Rating = state.Current.Rating.Value / (float)20;
                }
                // volumeSeekBar.Progress = state.Volume.Value;
                // songSeekBar.Progress = 

                playButton.SetImageResource(state.Playing == 1 ? Resource.Drawable.media_pause : Resource.Drawable.media_play);
            });
        }

        protected void PushNewPlayerState (Player state) {
            gmb.PushNewPlayerState(state).ContinueWith ((playerResult) => {
                if(playerResult.IsFaulted) {
                    Log.WriteLine(LogPriority.Error, c, "Problem pushing player state: " + playerResult.Exception);
                } else {
                    HandleUpdatedState(playerResult.Result);
                }
            });
        }

        protected void UpdateFromServer() { 
            gmb.FetchCurrentPlayerState().ContinueWith((playerResult) => {
                if(playerResult.IsFaulted) {
                    Log.WriteLine(LogPriority.Error, c, "Problem fetching player state: " + playerResult.Exception);
                } else {
                    HandleUpdatedState(playerResult.Result);
                }
            });
        }

        protected override void OnCreate (Bundle bundle) {
            base.OnCreate (bundle);

            gmb = new Gmusicbrowser();

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            playButton = FindViewById<ImageButton> (Resource.Id.PlayButton);
            titleTextView = FindViewById <TextView> (Resource.Id.TitleTextView);
            artistTextView = FindViewById <TextView> (Resource.Id.ArtistTextView);
            ratingBar = FindViewById <RatingBar> (Resource.Id.RatingBar);
            nextButton = FindViewById <ImageButton> (Resource.Id.NextButton);
            prevButton = FindViewById <ImageButton> (Resource.Id.PrevButton);
            volumeSeekBar = FindViewById <SeekBar> (Resource.Id.VolumeSeekBar);
            songSeekBar = FindViewById <SeekBar> (Resource.Id.SongSeekbar);

            playButton.Click += delegate {
                // button.Text = string.Format ("{0} clicks!", count++);
                gmb.PushNewPlayerState(new Player() { Playing = 1 });
            };

            ratingBar.RatingBarChange += (sender, e) => {
                // PushNewPlayerState (new PlayerState() {
            };

            nextButton.Click += (sender, e) => {
                // PushNewPlayerState(new PlayerState() {
            };

            volumeSeekBar.ProgressChanged += (sender, e) => {
                gmb.PushNewPlayerState (new Player() { Volume = volumeSeekBar.Progress / (float)100 });
            };

            // Architecture questions:

            // On Android, it often makes sense to have all of the data layer inside a Service, and then the Views and Controllers use IPC
            // (such as via a ContentProvider, or through your own custom Binder) to talk to it.

            // the presence of offline syncing makes a big difference here.

            // if offline syncing
        }

        protected override void OnResume () {
            base.OnResume ();
            Log.WriteLine(LogPriority.Info, c, "RESUME'D");
            UpdateFromServer ();
        }

        protected override void OnPause () {
            base.OnPause ();
            Log.WriteLine (LogPriority.Info, c, "PAUSE'D");

        }
    }
}

