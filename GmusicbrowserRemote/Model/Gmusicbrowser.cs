using System;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Android.Util;
using System.Threading.Tasks;

namespace GmusicbrowserRemote
{
    public class Gmusicbrowser
    {
        readonly string c = "Gmusicbrowser";

        RestClient gmbClient;


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

        public Task<Player> PushNewPlayerState (Player state) {
            var req = new RestRequest("/player", Method.POST);
            var task = new TaskCompletionSource<Player>();
            req.AddParameter("text/json", JsonConvert.SerializeObject(state, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore }), ParameterType.RequestBody);
            gmbClient.ExecuteAsync (req, (response) => {
                if(response.ResponseStatus == ResponseStatus.Error) {
                    var err = "Network problem posting Player state change to GMB: " + response.ErrorMessage;
                    Log.WriteLine(LogPriority.Error, c, err);
                    task.SetException (new Exception(err));
                } else if(response.StatusCode != System.Net.HttpStatusCode.OK) {
                    var err = "Problem posting Player state change to GMB: " + response.StatusCode + " - " + response.ErrorMessage;
                    Log.WriteLine(LogPriority.Error, c, err);
                    task.SetException (new Exception(err));
                } else {
                    task.SetResult (DeserializePlayer(response.Content));
                }
            });
            return task.Task;
        }

        public Task<Player> FetchCurrentPlayerState () {
            var req = new RestRequest ("/player", Method.GET);

            var task = new TaskCompletionSource<Player>();
            gmbClient.ExecuteAsync (req, (response) => {
                if(response.ResponseStatus == ResponseStatus.Error) {
                    var err = "Network problem reading status from GMB: " + response.ErrorMessage + ", " + response.Content + ", " + response.ErrorException;
                    Log.WriteLine(LogPriority.Error, c, err);
                    task.SetException(new Exception(err));
                } else if (response.StatusCode != System.Net.HttpStatusCode.OK) {
                    var err = "Problem reading status from GMB: " + response.ErrorMessage + ", " + response.StatusCode + ", " + response.ErrorException;
                    Log.WriteLine(LogPriority.Error, c, err);
                    task.SetException(new Exception(err));
                } else {
                    task.SetResult (DeserializePlayer(response.Content));
                }
            });

            return task.Task;
        }
    }
}
