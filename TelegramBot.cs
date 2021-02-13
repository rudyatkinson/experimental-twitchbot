using System;
using Telegram.Bot;
using System.Collections.Generic;

namespace Granzwelt.TwitchBot
{
    public class TelegramBot
    {
        public TelegramBotClient client {get; private set;}
        public Telegram.Bot.Types.User botInfo {get; private set;}
        
        private string accessToken;
        private string chatId;
        private string notificationMessage;
        private bool autoNotification;

        public void Connect()
        {
            if(!GetSettings())
            {
                Console.Clear();
                Console.WriteLine("Telegram config ayarları yapılmadığı için özellikler devredışı kalacak.\n" + 
                Settings.filePath +
                "\n\nDevam etmek için bir tuşa basınız.");
                Console.ReadKey();
                return;
            }

            client = new TelegramBotClient(accessToken);
            botInfo = client.GetMeAsync().Result;

            client.StartReceiving();

            if(autoNotification) SendStreamNotification(notificationMessage);
        }

        public async void SendStreamNotification(string message)
        {
            if(!String.IsNullOrEmpty(message))
            {
                await client.SendTextMessageAsync(
                    chatId: chatId,
                    text: message);
            }
        }

        private bool GetSettings()
        {
            string[] settings = Settings.GetConfig(Settings.ConfigFile.Telegram);
            if(settings == null) return false;

            int index = 0;
            foreach(string s in settings)
            {
                string[] parts = s.Split('=');
                if(parts.Length > 1)
                {
                    settings[index++] = parts[1].Trim();
                }
                else settings[index++] = string.Empty;
            }
            if(String.IsNullOrEmpty(settings[0]) || 
            String.IsNullOrEmpty(settings[1]) || 
            String.IsNullOrEmpty(settings[2]) ||
            String.IsNullOrEmpty(settings[3])) return false;

            accessToken = settings[0];
            chatId = settings[1];
            notificationMessage = settings[2];
            autoNotification = bool.Parse(settings[3]);

            return true;
        }
    }
}