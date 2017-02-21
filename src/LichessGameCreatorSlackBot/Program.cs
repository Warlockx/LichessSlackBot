using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LichessGameCreatorSlackBot.Services;

namespace LichessGameCreatorSlackBot
{
    public class Program
    {

        public static void Main(string[] args)
        {

            Slack slack = new Slack();
            Console.WriteLine("is authed = " + slack.TestAuthentication().Result);
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        slack.GetMessages().Wait();
                        Thread.Sleep(1000);
                    }
               
                });
            Console.WriteLine("Bot Running...");
           // 
            Console.ReadKey();
        }
    }
}
