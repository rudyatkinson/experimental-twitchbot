using System;
using System.Threading;
using System.Linq;

namespace TwitchBot.Granzwelt
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new Bot();
            Thread twitchThread = new Thread(bot.Connect);
            twitchThread.Start();
            twitchThread.Join();
            
            if (!bot.client.IsConnected)
            {
                Console.WriteLine($"Bot kanala bağlanamadı. Tekrar başlatmayı deneyiniz.\nKapatmak için bir tuşa basınız...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            TelegramBot telegramBot = new TelegramBot();
            Thread telegramThread = new Thread(telegramBot.Connect);
            telegramThread.Start();
            telegramThread.Join();

            Console_OnConnected(bot);

            string key = String.Empty;
            do
            {
                key = Console.ReadLine().ToLower();
                if (key != "e") bot.Chat(key);
                else bot.Chat($"{bot.GetBotName} kanaldan ayrıldı!");
            } while (key != "e");

            bot.userDictionary.SaveDictionary(bot.channelName, "UserDictionary");
            bot.commandDictionary.SaveDictionary(bot.channelName, "CommandDictionary");
            bot.counterDictionary.SaveDictionary(bot.channelName, "CounterDictionary");
            bot.listDictionary.Keys.ToList().SaveList(bot.channelName, "ListDictionaryAsList");
            bot.externalCommandList.SaveList(bot.channelName, "ExternalCommandsList");
            bot.timedCommandDictionary.SaveTimedMessages(bot.channelName, "TimedCommands");
        }

        private static void Console_OnConnected(Bot bot)
        {
            Console.Clear();
            Console.WriteLine($"{bot.channelName} kanalına bağlanıldı. \nÇıkış için E yazınız.");
        }
    }
}