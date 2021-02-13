using System;
using System.IO;
using System.Collections.Generic;

namespace Granzwelt.TwitchBot
{
    public static class Settings
    {
        public static string filePath {get; private set;} = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Granzwelt/RudyAsBot/Configs/";
        public enum ConfigFile
        {
            Twitch,
            Telegram
        }

        static string[] telegramConfigs = new string[]
        {
            "AccessToken",
            "ChatId",
            "NotificationMessage",
            "AutoNotification"
        };

        static string[] twitchConfigs = new string[]
        {
            "ChannelName",
            "BotName",
            "AccessToken"
        };

        public static string[] GetConfig(ConfigFile configFileType)
        {
            if (File.Exists($"{filePath}{configFileType.ToString()}.cfg"))
            {
                List<string> configs = new List<string>();
                using (StreamReader reader = new StreamReader($"{filePath}{configFileType.ToString()}.cfg"))
                {
                    while (!reader.EndOfStream) configs.Add(reader.ReadLine());
                }
                return configs.ToArray();
            }

            switch (configFileType)
            {
                case ConfigFile.Twitch:
                    CreateConfig(configFileType, twitchConfigs);
                    break;
                case ConfigFile.Telegram:
                    CreateConfig(configFileType, telegramConfigs);
                    break;
            }

            return null;
        }

        public static void CreateConfig(ConfigFile configFileType, params string[] configs)
        {
            Directory.CreateDirectory($"{filePath}");
            using (StreamWriter writer = new StreamWriter($"{filePath}{configFileType.ToString()}.cfg"))
            {
                foreach (string s in configs) writer.WriteLine($"{s}=");
            }
        }
    }
}