using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LichessGameCreatorSlackBot.Model
{
    public class SlackSettings
    {
        public string ChannelKey { get; }
        public string ApiKey { get; }
        public string LastReadMessage { get; }
        public string BotUserId { get; }
        public string BotName{ get; }

        public SlackSettings(string channelKey, string apiKey, string lastReadMessage,string botUserId,string botName)
        {
            ChannelKey = channelKey;
            ApiKey = apiKey;
            LastReadMessage = lastReadMessage;
            BotUserId = botUserId;
            BotName = botName;
        }
    }
}
