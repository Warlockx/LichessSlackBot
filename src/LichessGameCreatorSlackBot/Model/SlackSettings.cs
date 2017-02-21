using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LichessGameCreatorSlackBot.Model
{
    public class SlackSettings
    {
        public string ChannelKey { get; set; }
        public string ApiKey { get; set; }
        public string LastReadMessage { get; set; }
        public string BotUserId { get; set; }

        public SlackSettings(string channelKey, string apiKey, string lastReadMessage,string botUserId)
        {
            ChannelKey = channelKey;
            ApiKey = apiKey;
            LastReadMessage = lastReadMessage;
            BotUserId = botUserId;
        }
    }
}
