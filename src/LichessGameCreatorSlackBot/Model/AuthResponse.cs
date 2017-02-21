using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LichessGameCreatorSlackBot.Model
{
    public class AuthResponse
    {
            public bool ok { get; set; }
            public string url { get; set; }
            public string team { get; set; }
            public string user { get; set; }
            public string team_id { get; set; }
            public string user_id { get; set; }
    }
}
