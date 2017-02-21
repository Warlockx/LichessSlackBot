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
        private string _timeControlHelpMessage = ">The possible Time Modes are 'RealTime', 'Correspondence', 'Unlimited'.";
        private string _colorHelpMessage = ">The possible colors are 'Random', 'Black', 'White'.";
        private string _gameVariantHelpMessage = ">The possible variants are 'Standard', 'Crazyhouse', 'Chess960', 'KingOfTheHill', 'ThreeCheck', 'AntiChess', 'Atomic', 'Horde', 'RacingKings', 'FromPosition'.";
        private string _helpMessage = ">To create a new game with default settings type !chess \n" +
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

      
        private async Task ParseMessage(Message message)
        {
            StringBuilder response = new StringBuilder();
            string content = message.text.ToLower();
            if (content.Contains("new"))
            {
                response.Append(Lichess.CreateGame(content).Result);
                await PostMessage(response.ToString());
                return;
            }
            if (content.Contains("color"))
                response.Append(_colorHelpMessage+Environment.NewLine);
            if (content.Contains("timecontrol") || content.Contains("timemode"))
                response.Append(_timeControlHelpMessage + Environment.NewLine);
            if (content.Contains("gamevariant") || content.Contains("variant"))
                response.Append(_gameVariantHelpMessage + Environment.NewLine);
            if(content.Contains("Standard"))
                response.Append(ChessVariantInfo.GetInfo(ChessGameVariants.Standard) + Environment.NewLine);
            if (content.Contains("Crazyhouse"))
                response.Append(ChessVariantInfo.GetInfo(ChessGameVariants.Crazyhouse) + Environment.NewLine);
            if (content.Contains("KingOfTheHill"))
                response.Append(ChessVariantInfo.GetInfo(ChessGameVariants.KingOfTheHill) + Environment.NewLine);
            if (content.Contains("ThreeCheck"))
                response.Append(ChessVariantInfo.GetInfo(ChessGameVariants.ThreeCheck) + Environment.NewLine);
            if (content.Contains("AntiChess"))
                response.Append(ChessVariantInfo.GetInfo(ChessGameVariants.AntiChess) + Environment.NewLine);
            if (content.Contains("Atomic"))
                response.Append(ChessVariantInfo.GetInfo(ChessGameVariants.Atomic) + Environment.NewLine);
            if (content.Contains("Horde"))
                response.Append(ChessVariantInfo.GetInfo(ChessGameVariants.Horde) + Environment.NewLine);
            if (content.Contains("RacingKings"))
                response.Append(ChessVariantInfo.GetInfo(ChessGameVariants.RacingKings) + Environment.NewLine);
            if (content.Contains("FromPosition"))
                response.Append(ChessVariantInfo.GetInfo(ChessGameVariants.FromPosition) + Environment.NewLine);
            else if(content.Equals("!chess"))
                response.Append(_helpMessage);


            await PostMessage(response.ToString());
        }

        public async Task GetMessages()
        {
            string useLatest = _lastReadMessage != string.Empty ? $"&oldest={_lastReadMessage}" : string.Empty;
            
            HttpResponseMessage response =
                await _httpClient.GetAsync($"channels.history?token={_apiKey}&channel={_channelKey}{useLatest}");

          
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Error while getting new history messages.");

           
           string responseContent = await response.Content.ReadAsStringAsync();

            HistoryMessageResponse historyMessage =
                JsonConvert.DeserializeObject<HistoryMessageResponse>(responseContent);
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
                await GetMessages();
            else
                SaveSettings();
            
        }

        public async Task PostMessage(string message)
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync($"chat.postMessage?token={_apiKey}&channel={_channelKey}&text={message}&username={_botName}");
          
            Console.WriteLine($"Message sent | status:{response.StatusCode} response {await response.Content.ReadAsStringAsync()}.");


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
