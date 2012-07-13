using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using System.Threading.Tasks;
using GmusicbrowserRemote.Core;

namespace GmusicbrowserRemote
{
    [Activity (Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/gmusicbrowser")]
    public class RemoteActivity : Activity
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

        ProgressDialog prog;

        private Player currentState;

        private bool preventSeekBarUpdates = false;

        protected void HandleUpdatedStateFromNetwork (Player state) {
            Log.WriteLine(Android.Util.LogPriority.Info, c, String.Format ("SUCCESS, CURRENTLY {0}: {1} " , state.Playing ==  1 ? "Playing" : "Stopped", state.Current.Title));
            RunOnUiThread(() => {
                currentState = state;
                preventSeekBarUpdates = true;
                titleTextView.SetText (state.Current.Title, TextView.BufferType.Normal);
                artistTextView.SetText (state.Current.Artist, TextView.BufferType.Normal);
                if(state.Current != null) {
                    if (state.Current.Rating.HasValue) {
                        ratingBar.Rating = state.Current.Rating.Value / (float)20;
                    }

                    songSeekBar.Max = (int)state.Current.Length;
                    songSeekBar.Progress = (int)state.PlayPosition;
                }
                volumeSeekBar.Progress = (int)state.Volume.Value; // volume always has a value

                playButton.SetImageResource(state.Playing == 1 ? Resource.Drawable.media_pause : Resource.Drawable.media_play);
                preventSeekBarUpdates = false;
            });
        }

        protected void PushNewPlayerState (Player state) {
            gmb.PushNewPlayerState(state).ContinueWith ((playerResult) => {
                if(playerResult.IsFaulted || playerResult.IsCanceled) {
                    Log.WriteLine(LogPriority.Error, c, "Problem pushing player state: " + playerResult.Exception);
                    // TODO: reflect error in UI
                } else {
                    HandleUpdatedStateFromNetwork(playerResult.Result);
                }
            });
        }

        protected Task UpdateFromServer() {
            var task = new TaskCompletionSource<bool>();
            gmb.FetchCurrentPlayerState().ContinueWith((playerResult) => {
                if(playerResult.IsFaulted || playerResult.IsCanceled) {
                    Log.WriteLine(LogPriority.Error, c, "Problem fetching player state: " + playerResult.Exception);
                    // TODO: reflect error in UI
                } else {
                    HandleUpdatedStateFromNetwork(playerResult.Result);
                    task.SetResult(true);
                }
            });
            return task.Task;
        }

        protected override void OnCreate (Bundle bundle) {
            base.OnCreate (bundle);

            gmb = new Gmusicbrowser("macdesktop.orospakr.ca", 8081);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

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
                gmb.PushNewPlayerState(new Player() { Playing = currentState.Playing == 1 ? 0 : 1 }).ContinueWith((playerResult) => {
                    if(playerResult.IsFaulted) {
                    } else {
                        HandleUpdatedStateFromNetwork(playerResult.Result);
                    }
                });
            };

            ratingBar.RatingBarChange += (sender, e) => {
                if(currentState != null && currentState.Current != null) {
                    var newSong = new Song() { Id = currentState.Current.Id, Rating = (int)(ratingBar.Rating * 20)};
                    gmb.PostUpdatedSong(newSong);
                }
            };

            nextButton.Click += (sender, e) => {
                gmb.Next().ContinueWith((playerResult) => {
                    HandleUpdatedStateFromNetwork(playerResult.Result);
                });
            };

            prevButton.Click += (sender, e) => {
                gmb.Previous ().ContinueWith((playerResult) => {
                    HandleUpdatedStateFromNetwork(playerResult.Result);
                });
            };

            volumeSeekBar.ProgressChanged += (sender, e) => {
                if(!preventSeekBarUpdates) {
                    gmb.PushNewPlayerState (new Player() { Volume = volumeSeekBar.Progress / (float)100 });
                }
            };

            songSeekBar.ProgressChanged += (sender, e) => {
                if(!preventSeekBarUpdates) {
                    gmb.PushNewPlayerState (new Player() { PlayPosition = songSeekBar.Progress });
                }
            };

            prog = new ProgressDialog (this);
            prog.SetProgressStyle(ProgressDialogStyle.Spinner);
            prog.SetMessage(this.GetString(Resource.String.connection_progress));
            prog.Show ();
        }

        protected override void OnResume () {
            base.OnResume ();
            Log.WriteLine(LogPriority.Info, c, "RESUME'D");
            UpdateFromServer ().ContinueWith((taskResult) => {
                if(prog != null) prog.Dismiss();
                prog = null;
            });
        }

        protected override void OnPause () {
            base.OnPause ();
            Log.WriteLine (LogPriority.Info, c, "PAUSE'D");

        }
    }
}

