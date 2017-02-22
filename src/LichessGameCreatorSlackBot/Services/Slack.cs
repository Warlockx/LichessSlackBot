using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LichessGameCreatorSlackBot.Model;
using Newtonsoft.Json;

namespace LichessGameCreatorSlackBot.Services
{
    public class Slack
    {
        private static HttpClient _httpClient;
        private string _apiKey;
        private string _channelKey;
        private string _lastReadMessage = string.Empty;
        private string _botUserId;
        private string _botName;
        #region Responses
        private readonly string _timeControlHelpMessage = ">The possible Time Modes are 'RealTime', 'Correspondence', 'Unlimited'.";
        private readonly string _colorHelpMessage = ">The possible colors are 'Random', 'Black', 'White'.";
        private readonly string _gameVariantHelpMessage = ">The possible variants are 'Standard', 'Crazyhouse', 'Chess960', 'KingOfTheHill', 'ThreeCheck', 'AntiChess', 'Atomic', 'Horde', 'RacingKings', 'FromPosition'.";
        private readonly string _helpMessage = ">To create a new game with default settings type !chess new \n" +
                                      ">or customize the game with the following options '!chess new color:{Color}, timemode:{TimeControl}, variant:{GameVariant}, fen:{string}, increment:{int}, time:{double}'\n" +
                                      ">you can also ask about the option variables by saying !chess {variable}.";
        #endregion
        private readonly string _settingsFileLocation = Directory.GetCurrentDirectory()+"\\SlackVariables.json";
        public Slack()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://slack.com/api/");
            LoadSettings();
        }

        private void LoadSettings()
        {
            string test = Directory.GetCurrentDirectory();

            using (Stream fileStream = File.OpenRead(_settingsFileLocation))
            {
                if (fileStream == null)
                {
                    Console.WriteLine("Failed to load SlackVariables.json");
                    return;
                }
                try
                {
                    StreamReader reader = new StreamReader(fileStream);
                    string json = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(json))
                    {
                        Console.WriteLine("SlackVariables.Json is empty");
                        return;
                    }

                    SlackSettings slackSettings = JsonConvert.DeserializeObject<SlackSettings>(json);
                    _apiKey = slackSettings.ApiKey;
                    _channelKey = slackSettings.ChannelKey;
                    _lastReadMessage = slackSettings.LastReadMessage;
                    _botUserId = slackSettings.BotUserId;
                    _botName = slackSettings.BotName;
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }

        private void SaveSettings()
        {
            SlackSettings newSettings = new SlackSettings(_channelKey, _apiKey, _lastReadMessage,_botUserId,_botName);
            using (Stream fileStream = File.OpenWrite(_settingsFileLocation))
            {
                StreamWriter writer = new StreamWriter(fileStream);

                try
                {
                    string json = JsonConvert.SerializeObject(newSettings, Formatting.Indented);
                    writer.Write(json);
                    writer.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
              
            }
        }

        private void ParseMessageParams(ref StringBuilder response,string param)
        {
            switch (param)
            {
                case "color":
                    response.Append(_colorHelpMessage + Environment.NewLine);
                    break;
                case "timecontrol":
                case "timemode":
                    response.Append(_timeControlHelpMessage + Environment.NewLine);
                    break;
                case "gamevariant":
                case "variant":
                    response.Append(_gameVariantHelpMessage + Environment.NewLine);
                    break;
                default:
                {
                    object variant = param.GetEnum<ChessGameVariants>();
                    if (variant != null)
                    {
                        response.Append(ChessVariantInfo.GetInfo((ChessGameVariants)variant) + Environment.NewLine);
                        break;
                    }

                    if (!response.ToString().Contains(_helpMessage))
                        response.Append(_helpMessage + Environment.NewLine);
                    break;
                }
            }
        }

        private async Task ParseMessage(Message message)
        {
            StringBuilder response = new StringBuilder();
            string[] content = message.text.ToLower().Substring(6).Split(new [] {","," ", ":"},StringSplitOptions.RemoveEmptyEntries);

            if (message.text.ToLower().Contains("new"))
            {
                response.Append(Lichess.CreateGame(message.text.ToLower()).Result);
                await PostMessage(response.ToString());
                return;
            }
            foreach (string param in content)
            {
                if (!string.IsNullOrEmpty(param))
                    ParseMessageParams(ref response,param);
            }
            await PostMessage(response.ToString());
        }

        public async Task GetMessages()
        {
            while (true)
            {
                string useLatest = _lastReadMessage != string.Empty ? $"&oldest={_lastReadMessage}" : string.Empty;

                HttpResponseMessage response = await _httpClient.GetAsync($"channels.history?token={_apiKey}&channel={_channelKey}{useLatest}");


                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException("Error while getting new history messages.");


                string responseContent = await response.Content.ReadAsStringAsync();

                HistoryMessageResponse historyMessage = JsonConvert.DeserializeObject<HistoryMessageResponse>(responseContent);
                string lastMessageId = historyMessage?.messages?.FirstOrDefault()?.ts;
                if (!string.IsNullOrEmpty(lastMessageId))
                    _lastReadMessage = lastMessageId;

                IEnumerable<Message> botMessages = historyMessage?.messages?.Where(m => m.text.StartsWith("!chess"));

                if (botMessages != null)
                {
                    foreach (Message botMessage in botMessages)
                    {
                        await ParseMessage(botMessage);
                    }
                }
                if (historyMessage != null && historyMessage.has_more)
                    continue;
                SaveSettings();
                break;
            }
        }

        private async Task PostMessage(string message)
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync($"chat.postMessage?token={_apiKey}&channel={_channelKey}&text={message}&username={_botName}");
          
            Console.WriteLine($"Message sent | status: {response.StatusCode} response: {await response.Content.ReadAsStringAsync()}.");


        }
        public async Task<bool> TestAuthentication()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"auth.test?token={_apiKey}");

            if (!response.IsSuccessStatusCode) return false;

            string responseContent = await response.Content.ReadAsStringAsync();
            AuthResponse authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseContent);

            return authResponse.ok;
        }
    }
}
