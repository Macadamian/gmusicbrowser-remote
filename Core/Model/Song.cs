using System;

namespace GmusicbrowserRemote.Core
{
    public class Song
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public int? Length {get; set; }

        public int? Rating { get; set; }
    }
}
