using System;

namespace TwitchBot.Granzwelt
{
    public static class GetInfo
    {
        public static string GetChannelName()
        {
            Console.Write("Twitch Username: ");
            return SendFeedback();
        }

        private static string SendFeedback()
        {
            while (true)
            {
                string temp = Console.ReadLine().Trim();

                if (temp.Length > 0) return temp;
                else
                {
                    Console.Clear();
                    Console.Write("Bir şeyler yanlış gitti, lütfen tekrar dene: ");
                }
            }
        }
    }
}