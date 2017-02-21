using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LichessGameCreatorSlackBot
{
    public static class EnumeratorConvetor
    {
        public static T GetEnum<T>(this string s)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), Regex.Replace(s, @"\s+", ""),true);
            }
            catch (Exception e)
            {
              Console.WriteLine($"Exception raised at the function {nameof(GetEnum)} : {e.Message}");
            }
            
            return default(T);
        }
    }
}
