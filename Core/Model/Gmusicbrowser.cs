using System;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
// using Android.Util;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace GmusicbrowserRemote.Core
{
    /// <summary>
    /// Resolves member mappings for a type, into lowercase underscored forms.
    /// 
    /// eg. "camelCase" or "CamelCase" becomes "camel_case".
    /// </summary>
    public class UnderscorePropertyNamesResolver : DefaultContractResolver
    {
        Regex regex;

        public UnderscorePropertyNamesResolver()
            : base(true)
        {
            // http://stackoverflow.com/questions/4511087/regex-convert-camel-case-to-all-caps-with-underscores
            regex = new Regex("([A-Z])([A-Z][a-z])|([a-z0-9])([A-Z])");
        }

        protected override string ResolvePropertyName (string propertyName) {
            if ("PlayPosition".Equals (propertyName)) {
                // HACK: deal with an *mcs compiler crash* (?!) caused by putting the JsonProperty attribute on the PlayPosition field of Player directly.
                return "playposition";
            }
            return regex.Replace(propertyName, "$1$3_$2$4").ToLowerInvariant();
        }
    }

    public class Gmusicbrowser
    {
        readonly string c = "Gmusicbrowser";

        public string Hostname { get; private set; }
        public UInt16 Port { get; private set; }

        RestClient gmbClient;

        public Gmusicbrowser (string hostname, UInt16 port) {
            var uri = new UriBuilder ();
            Hostname = hostname;
            Port = port;
            uri.Host = hostname;
            uri.Port = port;
            uri.Scheme = "http";
            try {
                new Uri (uri.ToString ()); // throw an exception if the URI is invalid
            } catch {
                throw new FormatException("Invalid hostname or port");
            }
            gmbClient = new RestClient (uri.ToString());
        }

        // I have this nullable presence checking in the Deserialize* methods because I made
        // the fields nullable in the models for the purpose of doing selective updates

        public Player DeserializePlayer (string playerJson) {
            try {
                var player = JsonConvert.DeserializeObject<Player>(playerJson);
                if(player.Volume == null) {
                    throw new FormatException("Volume field should not be empty in Player.");
                }
                if(player.Playing == null) {
                    throw new FormatException("Playing field should not be empty in Player.");
                }
                return player;
            } catch (Exception e) {
                var err = "Problem decoding Player state JSON from GMB: " + e;
                // FIXME Log.WriteLine (LogPriority.Error, c, err);
                throw new FormatException(err);
            }
        }

        public Song DeserializeSong (string songJson) {
            try {
                var song = JsonConvert.DeserializeObject<Song>(songJson);
                if(song.Length == null) {
                    throw new FormatException("Length field should not be empty in Song.");
                }
                return song;
            } catch (Exception e) {
                var err = "Problem decoding Player state JSON from GMB: " + e;
                // FIXME Log.WriteLine (LogPriority.Error, c, err);
                throw new FormatException(err);
            }
        }

        public Task<Player> PushNewPlayerState (Player state) {
            var req = new RestRequest("/player", Method.POST);

            req.AddParameter("text/json", JsonConvert.SerializeObject(state, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new UnderscorePropertyNamesResolver(), NullValueHandling = NullValueHandling.Ignore }), ParameterType.RequestBody);

            return ExecuteHttpTaskThatReturnsPlayerState(req, "pushing player state");
        }

        private Task<Player> ExecuteHttpTaskThatReturnsPlayerState (RestRequest req, string why) {
            var task = new TaskCompletionSource<Player>();

            ExecuteHttpTask (req, why).ContinueWith((requestResult) => {
                try {
                    var player = DeserializePlayer(requestResult.Result);
                    task.SetResult (player);
                } catch (Exception e) {
                    task.SetException(e);
                }
            });

            return task.Task;
        }

        private Task<String> ExecuteHttpTask (RestRequest req, string why) {
            var task = new TaskCompletionSource<String>();
            gmbClient.ExecuteAsync (req, (response) => {
                if(response.ResponseStatus == ResponseStatus.Error) {
                    var err = String.Format ("Network problem {0} from GMB: {1}, {2}, {3}", why, response.ErrorMessage, response.Content, response.ErrorException);
                    // FIXME Log.WriteLine(LogPriority.Error, c, err);
                    task.SetException(new Exception(err));
                } else if (response.StatusCode != System.Net.HttpStatusCode.OK) {
                    var err = String.Format ("Problem {0} from GMB: {1}, {2}, {3}", why, response.ErrorMessage, response.StatusCode, response.ErrorException);
                    // FIXME Log.WriteLine(LogPriority.Error, c, err);
                    task.SetException(new Exception(err));
                } else {
                    task.SetResult (response.Content);
                }
            });
            return task.Task;
        }

        public Task<Player> FetchCurrentPlayerState () {
            var req = new RestRequest ("/player", Method.GET);

            return ExecuteHttpTaskThatReturnsPlayerState(req, "fetching state");
        }

        public Task<Player> Next() {
            var req = new RestRequest("/skip", Method.POST);

            return ExecuteHttpTaskThatReturnsPlayerState(req, "skipping to next track");
        }

        public Task<Player> Previous() {
            var req = new RestRequest("/prev", Method.POST);

            return ExecuteHttpTaskThatReturnsPlayerState(req, "returning to previous track");
        }

        /// <summary>
        /// Posts the updated song to the server.  It actually puts formencoded (?!) JSON into the request, which is how I wrote the server for some reason.
        /// </summary>
        /// <returns>
        /// The updated song.
        /// </returns>
        /// <param name='song'>
        /// Song.
        /// </param>
        public Task<Song> PostUpdatedSong(Song song) {
            var req = new RestRequest(String.Format("/songs/{0}", song.Id), Method.POST);
            var task = new TaskCompletionSource<Song>();
            req.AddParameter("song", JsonConvert.SerializeObject(song, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new UnderscorePropertyNamesResolver(), NullValueHandling = NullValueHandling.Ignore }));
            // req.AddParameter("song", JsonConvert
            ExecuteHttpTask(req, "pushing song").ContinueWith((songResult) => {
                try {
                    var updatedSong = DeserializeSong(songResult.Result);
                    task.SetResult (updatedSong);
                } catch (Exception e) {
                    task.SetException(e);
                }

            });
            return task.Task;
        }
    }
}
