using System;

namespace TwitchBot.Granzwelt
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new Bot();
            string key = String.Empty;
            
            do
            {
                Console.Clear();
                key = Console.ReadLine().ToLower();
                if (key != "e") bot.Chat(key);
                else bot.Chat($"{bot.GetBotName} kanaldan ayrıldı!");
            } while (key != "e");

            bot.userDictionary.SaveDictionary(bot.channelName, "UserDictionary");
            bot.commandDictionary.SaveDictionary(bot.channelName, "CommandDictionary");
            bot.externalCommandList.SaveList(bot.channelName, "ExternalCommandsList");
        }
    }
}