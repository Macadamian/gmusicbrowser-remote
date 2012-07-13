using System;

namespace GmusicbrowserRemote.Core
{
    public class Logging
    {
        public Logging () {
        }

        private static void WriteMessage (string level, string c, string msg) {
            System.Diagnostics.Debug.WriteLine(String.Format("{0}:{1}:{2}", level, c, msg));
        }

        public static void Error (string c, string msg) {
            WriteMessage("ERROR", c, msg);
        }

        public static void Info (string c, string msg) {
            WriteMessage("INFO", c, msg);
        }

        public static void Debug (string c, string msg) {
            WriteMessage("DEBUG", c, msg);
        }

        public static void Warn (string c, string msg) {
            WriteMessage("WARN", c, msg);
        }
    }
}

