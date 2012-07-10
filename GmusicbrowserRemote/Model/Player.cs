using System;

namespace GmusicbrowserRemote
{
    // // {"volume":"94","current":{"length":217,"artist":"Potshot","title":"Ultima 6 Gates of Creation OC ReMix","id":1779,"rating":80},"queue":[],"playing":1,"playposition":95.364219}
    public class Player
    {
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

        public Player () {
        }
    }
}

