using System;


namespace Twitch_chat_bot.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new TwitchChatBot();
            while (true) { Console.ReadLine(); }
        }
    }
}
