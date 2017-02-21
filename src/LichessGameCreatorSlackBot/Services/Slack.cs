﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        private string _gameVariantHelpMessage = ">The possible Time Modes are 'Standard', 'Crazyhouse', 'Chess960', 'KingOfTheHill', 'ThreeCheck', 'AntiChess', 'Atomic', 'Horde', 'RacingKings', 'FromPosition'.";
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
            string content = message.text.ToLower();
            if (content.Contains("new"))
            {
                await PostMessage(Lichess.CreateGame(content).Result);
                return;
            }
            if (content.Contains("color"))
                await PostMessage(_colorHelpMessage);
            else if (content.Contains("timecontrol") || content.Contains("timemode"))
                await PostMessage( _timeControlHelpMessage);
            else if (content.Contains("gamevariant") || content.Contains("variant"))
                await PostMessage(_gameVariantHelpMessage);
            else if(content.Contains("Standard"))
                await PostMessage(ChessVariantInfo.GetInfo(ChessGameVariants.Standard));
            else if (content.Contains("Crazyhouse"))
                await PostMessage(ChessVariantInfo.GetInfo(ChessGameVariants.Crazyhouse));
            else if (content.Contains("KingOfTheHill"))
                await PostMessage(ChessVariantInfo.GetInfo(ChessGameVariants.KingOfTheHill));
            else if (content.Contains("ThreeCheck"))
                await PostMessage(ChessVariantInfo.GetInfo(ChessGameVariants.ThreeCheck));
            else if (content.Contains("AntiChess"))
                await PostMessage(ChessVariantInfo.GetInfo(ChessGameVariants.AntiChess));
            else if (content.Contains("Atomic"))
                await PostMessage(ChessVariantInfo.GetInfo(ChessGameVariants.Atomic));
            else if (content.Contains("Horde"))
                await PostMessage(ChessVariantInfo.GetInfo(ChessGameVariants.Horde));
            else if (content.Contains("RacingKings"))
                await PostMessage(ChessVariantInfo.GetInfo(ChessGameVariants.RacingKings));
            else if (content.Contains("FromPosition"))
                await PostMessage(ChessVariantInfo.GetInfo(ChessGameVariants.FromPosition));
            else
                await PostMessage(_helpMessage);

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

            if (response.IsSuccessStatusCode) return;

            Console.WriteLine(await response.Content.ReadAsStringAsync());


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