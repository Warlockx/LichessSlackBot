using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LichessGameCreatorSlackBot.Model;

namespace LichessGameCreatorSlackBot
{
    public static class Lichess
    {
        private static HttpClient _httpClient;

        static Lichess()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://en.lichess.org/");
        }
        public static async Task<string> CreateGame(string settings)
        {
            Tuple<ChessColors, ChessTimeControl, ChessGameVariants,string,string,string> gameSettings = ParseSettings(settings);
            FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("color", gameSettings.Item1.ToString() ?? "random"),
                new KeyValuePair<string, string>("days", "1"),
                new KeyValuePair<string, string>("fen", gameSettings.Item4 ?? ""),
                new KeyValuePair<string, string>("increment", gameSettings.Item5 ?? "8"),
                new KeyValuePair<string, string>("time", gameSettings.Item6 ?? "5.0"),
                new KeyValuePair<string, string>("timeMode", ((int)gameSettings.Item2).ToString() ?? "0"),
                new KeyValuePair<string, string>("variant", ((int)gameSettings.Item3).ToString() ?? "1")
            });
            return
                $"Creating game with the settings color:{gameSettings.Item1}, fen:{gameSettings.Item4}, increment:{gameSettings.Item5}, time:{gameSettings.Item6}, timemode:{gameSettings.Item2}, variant:{gameSettings.Item3}";
            //  await _httpClient.PostAsync("/setup/friend", content);
        }

        public static Tuple<ChessColors,ChessTimeControl,ChessGameVariants,string,string,string> ParseSettings(string settings)
        {
            //!chess new color:{Color}, timemode:{TimeControl}, variant:{GameVariant}, fen:{string}, increment:{int}, time:{double}
            int settingsIndexStart = settings.IndexOf("new", StringComparison.Ordinal) + 4;

            if (settings.Length <= settingsIndexStart)
                return
                    new Tuple<ChessColors, ChessTimeControl, ChessGameVariants, string, string, string>(
                        ChessColors.Random, ChessTimeControl.Unlimited, ChessGameVariants.Standard,
                        "", "8", "5.0");



            string[] settingStrings = settings.Substring(settingsIndexStart).ToLower().Trim().Split(',');
            string color = string.Empty;
            string timemode = string.Empty;
            string variant = string.Empty;
            string fen = string.Empty;
            string increment = "8";
            string time = "5.0";
            foreach (string setting in settingStrings)
            {
                if (setting.Contains("color"))
                    color = setting.Substring(6, setting.Length - 6);
                else if (setting.Contains("timemode"))
                    timemode = setting.Substring(10, setting.Length - 10);
                else if (setting.Contains("variant"))
                    variant = setting.Substring(9, setting.Length - 9);
                else if (setting.Contains("fen"))
                    fen = setting.Substring(4, setting.Length - 4);
                else if (setting.Contains("increment"))
                    increment = setting.Substring(10, setting.Length - 10);
                else if (setting.Contains("time"))
                    time = setting.Substring(5, setting.Length - 5);
            }

            ChessColors chessColors = string.IsNullOrEmpty(color) ? ChessColors.Random : color.GetEnum<ChessColors>();
            ChessTimeControl chessTimeControl = string.IsNullOrEmpty(timemode) ? ChessTimeControl.Unlimited : timemode.GetEnum<ChessTimeControl>();
            ChessGameVariants chessGameVariants = string.IsNullOrEmpty(variant) ? ChessGameVariants.Standard : variant.GetEnum<ChessGameVariants>();

            return new Tuple<ChessColors, ChessTimeControl, ChessGameVariants,string,string,string>(chessColors,chessTimeControl,chessGameVariants,fen,increment,time);
        }
    }
}
