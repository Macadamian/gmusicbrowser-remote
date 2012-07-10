using System;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Android.Util;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace GmusicbrowserRemote
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
            return regex.Replace(propertyName, "$1$3_$2$4").ToLowerInvariant();
        }
    }

    public class Gmusicbrowser
    {
        readonly string c = "Gmusicbrowser";

        RestClient gmbClient;

        // TODO: work out how how to properly error-out TaskCompletionSources.


        public Gmusicbrowser () {
            gmbClient = new RestClient ("http://macdesktop.orospakr.ca:8081");
        }

        public Player DeserializePlayer (string playerJson) {
            try {
                return JsonConvert.DeserializeObject<Player>(playerJson);
            } catch (Exception e) {
                var err = "Problem decoding Player state JSON from GMB: " + e;
                Log.WriteLine (LogPriority.Error, c, err);
                throw new FormatException(err);
            }
        }

        public Song DeserializeSong (string songJson) {
            try {
                return JsonConvert.DeserializeObject<Song>(songJson);
            } catch (Exception e) {
                var err = "Problem decoding Player state JSON from GMB: " + e;
                Log.WriteLine (LogPriority.Error, c, err);
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
                task.SetResult (DeserializePlayer(requestResult.Result));
            });

            return task.Task;
        }

        private Task<String> ExecuteHttpTask (RestRequest req, string why) {
            var task = new TaskCompletionSource<String>();
            gmbClient.ExecuteAsync (req, (response) => {
                if(response.ResponseStatus == ResponseStatus.Error) {
                    var err = String.Format ("Network problem {0} from GMB: {1}, {2}, {3}", why, response.ErrorMessage, response.Content, response.ErrorException);
                    Log.WriteLine(LogPriority.Error, c, err);
                    task.SetException(new Exception(err));
                } else if (response.StatusCode != System.Net.HttpStatusCode.OK) {
                    var err = String.Format ("Problem {0} from GMB: {1}, {2}, {3}", why, response.ErrorMessage, response.StatusCode, response.ErrorException);
                    Log.WriteLine(LogPriority.Error, c, err);
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
                task.SetResult (DeserializeSong(songResult.Result));
            });
            return task.Task;
        }
    }
}
