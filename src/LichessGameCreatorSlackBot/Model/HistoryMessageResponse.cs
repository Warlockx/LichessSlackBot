using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LichessGameCreatorSlackBot.Model
{
        public class Reaction
        {
            public string name { get; set; }
            public int count { get; set; }
            public List<string> users { get; set; }
        }

        public class Message
        {
            public string type { get; set; }
            public string ts { get; set; }
            public string user { get; set; }
            public string text { get; set; }
            public bool? is_starred { get; set; }
            public List<Reaction> reactions { get; set; }
            public bool? wibblr { get; set; }
        }

        public class HistoryMessageResponse
        {
            public bool ok { get; set; }
            public string latest { get; set; }
            public List<Message> messages { get; set; }
            public bool has_more { get; set; }
        }
}
