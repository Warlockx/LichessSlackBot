using System;
using System.Text.RegularExpressions;

namespace LichessGameCreatorSlackBot.Services
{
    public static class EnumeratorConvetor
    {
        public static object GetEnum<T>(this string s)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), Regex.Replace(s, @"\s+", ""),true);
            }
            catch (Exception e)
            {
              Console.WriteLine($"Exception raised at the function {nameof(GetEnum)} : {e.Message}");
            }
            
            return null;
        }
    }
}
