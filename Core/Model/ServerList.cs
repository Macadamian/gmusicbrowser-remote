using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace GmusicbrowserRemote.Core
{
    public class ServerList
    {
        private readonly string c = "ServerList";
        private readonly String LIST_FILE_NAME = "gmbs.json";

        public ServerList () {
        }

        /// <summary>
        /// Returns the list of Gmusicbrowser instances
        /// </summary>
        /// <returns>
        /// The from storage.
        /// </returns>
        public Task<List<Gmusicbrowser>>LoadFromStorage () {
            var task = new TaskCompletionSource<List<Gmusicbrowser>> ();

            var store = IsolatedStorageFile.GetUserStoreForApplication ();
            if (store.FileExists (LIST_FILE_NAME)) {
                var fd = store.OpenFile (LIST_FILE_NAME, FileMode.Open);
                var gmbsData = new byte[fd.Length];
                fd.BeginRead (gmbsData, 0, (int)fd.Length, (asyncResult) => {
                    var gmbJson = Encoding.UTF8.GetString (gmbsData, 0, (int)fd.Length);
                    fd.Close ();
                    store.Dispose();
                    var list = JsonConvert.DeserializeObject<List<Gmusicbrowser>> (gmbJson);
                    task.SetResult(list);
                }, null);
            } else {
                var list = new List<Gmusicbrowser> ();
                task.SetResult (list);
            }

            return task.Task;
        }


        public Task WriteToStorage(List<Gmusicbrowser> list) {
            // this is trickier than it looks, because I want to guarantee both file-system level atomicity AND avoid delayed allocation sadness
            var task = new TaskCompletionSource<bool>();
            var json = JsonConvert.SerializeObject(list);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var store = IsolatedStorageFile.GetUserStoreForApplication ();
            var tempFile = LIST_FILE_NAME + "-" + System.Guid.NewGuid().ToString();
            var fd = store.OpenFile (tempFile, FileMode.CreateNew);

            fd.BeginWrite(jsonBytes, 0, jsonBytes.Length, (writeResult) => {
                try {
                    fd.Close();
                    if(store.FileExists(LIST_FILE_NAME)) {
                        store.DeleteFile(LIST_FILE_NAME); // HACK: augh, why oh why doesn't it let me replace file?!  You can tell this API was designed by windows people
                    }
                    store.MoveFile(tempFile, LIST_FILE_NAME);
                    store.Dispose();
                    task.SetResult(true);
                } catch (Exception e) {
                    Logging.Error(c, "Unable to write server list to storage: " + e);
                }
            }, null);
            return task.Task;
        }
    }
}
