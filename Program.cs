using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Granzwelt.TwitchBot.Unity;

namespace Granzwelt.TwitchBot
{
    class Program
    {
        static Bot bot;
        static TelegramBot telegramBot;
        static UnityConnection unityConnection;

        static void Main(string[] args)
        {
            bot = new Bot();
            Thread twitchThread = new Thread(bot.Connect);
            twitchThread.Start();
            twitchThread.Join();
            
            if (!bot.client.IsConnected)
            {
                Console.WriteLine($"Bot kanala bağlanamadı. Tekrar başlatmayı deneyiniz.\nKapatmak için bir tuşa basınız...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            telegramBot = new TelegramBot();
            Thread telegramThread = new Thread(telegramBot.Connect);
            telegramThread.Start();
            telegramThread.Join();

            Console_OnConnected(bot);

            unityConnection = new UnityConnection("TestConnection");
            // Thread unityThread = new Thread(unityConnection.CreateServer);
            // unityThread.Start();

            // TestClient test = new TestClient("TestConnection");
            // Thread testThread = new Thread(test.Connect);
            // testThread.Start();

            string key = String.Empty;
            bool exit = false;
            do
            {
                key = Console.ReadLine().Trim();
                if (key.Length > 0 && key.ElementAt(0) != '/') bot.Chat(key);
                else 
                {
                    string[] parts = key.Split(' ');
                    switch(parts[0])
                    {
                        case "/e":
                            bot.Chat($"{bot.GetBotName} kanaldan ayrıldı!");
                            exit = true;
                            
                            //unityConnection.DisposeServer();
                            break;
                        case "/isConnected":
                            Thread t1 = new Thread(() => Console.WriteLine(unityConnection.IsConnected()));
                            t1.Start();
                            // Thread t2 = new Thread(() => Console.WriteLine(test.IsConnected()));
                            // t2.Start();
                            break;
                        case "/createServer":
                            Thread t3 = new Thread(unityConnection.CreateServer);
                            t3.Start();
                            break;
                        // case "/connectToServer":
                        //     Thread t5 = new Thread(test.Connect);
                        //     t5.Start();
                        //     break;
                        case "/send":
                            string result = string.Empty;
                            for(int i = 1; i < parts.Length; i++)
                            {
                                result += parts[i] + " ";
                            }
                            Thread writeThread = new Thread(() => {
                                unityConnection.WriteString(result.Trim());
                            });
                            writeThread.Start();
                            break;
                        default:
                            Console.WriteLine("\nKomut algılanamadı.");
                            break;
                    }
                    
                }
            } while (!exit);

            bot.userDictionary.SaveDictionary(bot.channelName, "UserDictionary");
            bot.commandDictionary.SaveDictionary(bot.channelName, "CommandDictionary");
            bot.counterDictionary.SaveDictionary(bot.channelName, "CounterDictionary");
            bot.listDictionary.Keys.ToList().SaveList(bot.channelName, "ListDictionaryAsList");
            bot.externalCommandList.SaveList(bot.channelName, "ExternalCommandsList");
            bot.timedCommandDictionary.SaveTimedMessages(bot.channelName, "TimedCommands");
        }

        private static void Console_OnConnected(Bot bot)
        {
            // Console.Clear();
            // Console.WriteLine($"{bot.channelName} kanalına bağlanıldı. \nÇıkış için /e yazınız.");
        }
    }
}