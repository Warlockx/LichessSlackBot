using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LichessGameCreatorSlackBot
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Slack slack = new Slack();
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        slack.GetMessages().Wait();
                        Thread.Sleep(2000);
                    }
               
                });
           
            //Lichess.ParseSettings("!chess color:random        , timemode:Correspondence               , variant:Atomic");
           // Console.WriteLine("is authed = "+ slack.TestAuthentication().Result);
            Console.ReadKey();
        }
    }
}
