using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GmusicbrowserRemote
{
    public class Song {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public int Length {get; set; }

        public int? Rating { get; set; }

    }

    // {"volume":"94","current":{"length":217,"artist":"Potshot","title":"Ultima 6 Gates of Creation OC ReMix","id":1779,"rating":80},"queue":[],"playing":1,"playposition":95.364219}
    public class PlayerState {
        public Song Current { get; set; }

        /// <summary>
        /// Gets or sets the playing state.
        /// </summary>
        /// <value>
        /// 1 for playing, 0 for paused.
        /// </value>
        public int? Playing { get; set; }

        /// <summary>
        /// Gets or sets the volume.
        /// 
        /// NB. Only nullable so that serializing the class for posting can optionally include the value.  Incoming should always include it.
        /// </summary>
        /// <value>
        /// The volume, between 0 and 100 (on incoming), or between 0 and 1 (on outgoing, it's a bug in the GMB http-server plugin).
        /// </value>
        public float? Volume { get; set; }
        public double? PlayPosition { get; set; }
    }

    [Activity (Label = "GmusicbrowserRemote", MainLauncher = true)]
    public class Activity1 : Activity
    {
        readonly string c = "RemoteActivity";

        private RestClient gmbClient;

        TextView titleTextView;

        ImageButton playButton;

        TextView artistTextView;

        RatingBar ratingBar;

        ImageButton nextButton;

        ImageButton prevButton;

        SeekBar volumeSeekBar;

        SeekBar songSeekBar;

        protected void PushNewPlayerState (PlayerState state) {
            var req = new RestRequest("/player", Method.POST);
            req.AddParameter("text/json", JsonConvert.SerializeObject(state, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore }), ParameterType.RequestBody);
            gmbClient.ExecuteAsync (req, (response) => {
                if(response.ResponseStatus == ResponseStatus.Error) {
                    Log.WriteLine(LogPriority.Error, c, "Network problem posting Player state change to GMB: " + response.ErrorMessage);
                } else if(response.StatusCode != System.Net.HttpStatusCode.OK) {
                    Log.WriteLine(LogPriority.Error, c, "Problem posting Player state change to GMB: " + response.StatusCode + " - " + response.ErrorMessage);
                } else {
                    HandleUpdatedState (response.Content);
                }
            });
        }

        protected void HandleUpdatedState (PlayerState state) {
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

        protected void HandleUpdatedState (String state) {
            try {
                HandleUpdatedState(Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerState>(state));
            } catch (Exception e) {
                Log.WriteLine (LogPriority.Error, c, "Problem decoding Player state JSON from GMB: " + e);
            }
        }

        protected void UpdateFromServer() { 
            var req = new RestRequest ("/player", Method.GET);

            gmbClient.ExecuteAsync (req, (response) => {
                if(response.ResponseStatus == ResponseStatus.Error) {
                    Log.WriteLine(LogPriority.Error, c, "Network problem reading status from GMB: " + response.ErrorMessage + ", " + response.Content + ", " + response.ErrorException);
                } else if (response.StatusCode != System.Net.HttpStatusCode.OK) {
                    Log.WriteLine(LogPriority.Error, c, "Problem reading status from GMB: " + response.ErrorMessage + ", " + response.StatusCode + ", " + response.ErrorException);
                } else {
                    HandleUpdatedState(response.Content);
                }
            });
        }

        protected override void OnCreate (Bundle bundle) {
            base.OnCreate (bundle);

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

            gmbClient = new RestClient ("http://macdesktop.orospakr.ca:8081");

            playButton.Click += delegate {
                // button.Text = string.Format ("{0} clicks!", count++);
                PushNewPlayerState(new PlayerState() { Playing = 1 });
            };

            ratingBar.RatingBarChange += (sender, e) => {
                // PushNewPlayerState (new PlayerState() {
            };

            nextButton.Click += (sender, e) => {
                // PushNewPlayerState(new PlayerState() {
            };

            volumeSeekBar.ProgressChanged += (sender, e) => {
                PushNewPlayerState (new PlayerState() { Volume = volumeSeekBar.Progress / (float)100 });
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

