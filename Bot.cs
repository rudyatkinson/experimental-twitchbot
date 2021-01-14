using System;
using System.Collections.Generic;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using System.Linq;


namespace TwitchBot.Granzwelt
{
    struct User
    {
        public string channelName;
    }

    public class Bot
    {
        #region attributes

        public const string author = "rudyatkinson";
        public const string userName = "botName";
        public const string accessToken = "accessToken";
        public string channelName { get; private set; }

        #endregion

        #region runtime editable collections

        public Dictionary<string, string> userDictionary { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> commandDictionary { get; private set; } = new Dictionary<string, string>();
        public List<string> externalCommandList {get; private set;} = new List<string>();

        #endregion

        #region exclusive commands lists

        List<string> commandList = new List<string>()
        {
            "!komutlar",
            "!ben",
            "!kim",
            "!sarki"
        };

        List<string> modCommandList = new List<string>()
        {
            "!haricikomutekle",
            "!haricikomutsil",
            "!komutekle",
            "!komutsil"
        };

        #endregion

        TwitchClient client;

        public Bot()
        {
            var user = new User
            {
                channelName = GetInfo.GetChannelName()
            };

            ConnectionCredentials credentials = new ConnectionCredentials(userName, accessToken);

            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            var customClient = new WebSocketClient(clientOptions);

            client = new TwitchClient(customClient);
            client.Initialize(credentials, user.channelName, '!');

            client.OnConnected += Client_OnConnected;
            client.OnMessageReceived += Client_OnMessageReceived;

            client.Connect();
            channelName = user.channelName;

            userDictionary.LoadDictionary(channelName, "UserDictionary");
            commandDictionary.LoadDictionary(channelName, "CommandDictionary");
            externalCommandList.LoadList(channelName, "ExternalCommandsList");

            Console.WriteLine(externalCommandList.Count);
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            Console.Clear();
            Console.WriteLine($"{e.AutoJoinChannel} kanalına bağlanıldı. \nÇıkış için E yazınız.");
            client.SendMessage(e.AutoJoinChannel, $"{userName} kanala bağlandı! Komutları öğrenmek için !komutlar yazabilirsiniz.");
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message[0].Equals('!') && !e.ChatMessage.Message.Contains('@'))
            {
                // Replacing '@' character with a space and trim.
                string inputMessage = e.ChatMessage.Message.Replace('@', ' ').Trim();
                // Defining parts of the input given by chat.
                string[] parts = inputMessage.Split(' ');

                switch (parts[0])
                {
                    case "!ben":
                        CommandBen(e, parts);
                        break;
                    case "!kim":
                        CommandKim(e, parts);
                        break;
                    case "!komutekle":
                        CommandKomutEkle(e, parts);
                        break;
                    case "!komutsil":
                        CommandKomutSil(e, parts);
                        break;
                    case "!haricikomutekle":
                        CommandHariciKomutEkle(e, parts);
                        break;
                    case "!haricikomutsil":
                        CommandHariciKomutSil(e, parts);
                        break;
                    case "!komutlar":
                        CommandKomutlar(e);
                        break;
                    case "!sarki":
                        CommandSarki(e);
                        break;
                    default:
                        if (commandDictionary.ContainsKey($"{parts[0]}"))
                        {
                            client.SendMessage(channelName, commandDictionary[$"{parts[0]}"]);
                        }
                        break;
                }
            }
        }

        private void CommandKomutEkle(OnMessageReceivedArgs e, string[] parts)
        {
            if (!IsValidToModerate(e.ChatMessage)) return;

            if(parts.Length <= 2)
            {
                client.SendMessage(e.ChatMessage.Channel, $"Komut eklemek için (!komutekle <komut_adı> <komut açıklaması>) şeklinde giriş yapılmalıdır. Örneğin; !komutekle okul ağğğbi boğaziçi");
                return;
            }

            if (commandList.Contains($"!{parts[1]}") ||
            modCommandList.Contains($"!{parts[1]}"))
            {
                client.SendMessage(e.ChatMessage.Channel, $"Komut (!{parts[1]}) zaten mevcut.");
                return;
            }

            if (!commandDictionary.ContainsKey($"!{parts[1]}"))
            {
                commandDictionary.Add($"!{parts[1]}", null);
                client.SendMessage(e.ChatMessage.Channel, $"Komut (!{parts[1]}) eklendi.");
            }
            else client.SendMessage(e.ChatMessage.Channel, $"Komut (!{parts[1]}) güncellendi.");

            string temp = String.Empty;
            for (int i = 2; i < parts.Length; i++) temp += $"{parts[i]} ";

            if (externalCommandList.Contains($"!{parts[1]}")) externalCommandList.Remove($"!{parts[1]}");

            commandDictionary[$"!{parts[1]}"] = temp;
            commandDictionary.SaveDictionary(channelName, "CommandDictionary");
        }

        private void CommandKomutSil(OnMessageReceivedArgs e, string[] parts)
        {
            if(!IsValidToModerate(e.ChatMessage)) return;

            if(parts.Length <= 1)
            {
                client.SendMessage(e.ChatMessage.Channel, $"Komut silmek için (!komutsil <komut_adı>) şeklinde giriş yapılmalıdır. Örneğin; !komutsil okul");
                return;
            }

            string feedback = String.Empty;
            if(commandDictionary.ContainsKey($"!{parts[1]}"))
            {
                commandDictionary.Remove($"!{parts[1]}");
                feedback = $"Bu (!{parts[1]}) komut başarıyla silindi.";
            }
            else feedback = $"Böyle (!{parts[1]}) bir komut bulunmuyor.";

            client.SendMessage(e.ChatMessage.Channel, feedback);
            commandDictionary.SaveDictionary(channelName, "CommandDictionary");
        }

        private void CommandSarki(OnMessageReceivedArgs e)
        {
            client.SendMessage(e.ChatMessage.Channel, ProcessInfo.GetSpotifyInfo());
        }

        private void CommandHariciKomutEkle(OnMessageReceivedArgs e, string[] parts)
        {
            if (!IsValidToModerate(e.ChatMessage)) return;

            if(parts.Length <= 1)
            {
                client.SendMessage(e.ChatMessage.Channel, $"Harici komut, diğer botlarda zaten varolan komutları bu komut listesine eklemek için kullanılır. Kullanımı (!haricikomutekle <harici_komut_adı>) şeklinde yapılır. Örneğin; !haricikomutekle dc");
                return;
            }

            if (!externalCommandList.Contains($"!{parts[1]}") &&
            !commandList.Contains($"!{parts[1]}") &&
            !commandDictionary.ContainsKey($"!{parts[1]}") &&
            !modCommandList.Contains($"!{parts[1]}")) externalCommandList.Add($"!{parts[1]}");
            else 
            {
                client.SendMessage(e.ChatMessage.Channel, $"Bu (!{parts[1]}) komut zaten bulunmaktadır.");
                return;
            }

            externalCommandList.SaveList(channelName, "ExternalCommandsList");
            client.SendMessage(e.ChatMessage.Channel, $"Başka bir botta kullanılan bu ({parts[1]}) komut, komutlar listesine eklendi.");
        }

        private void CommandHariciKomutSil(OnMessageReceivedArgs e, string[] parts)
        {
            if(!IsValidToModerate(e.ChatMessage)) return;

            if(parts.Length <= 1)
            {
                client.SendMessage(e.ChatMessage.Channel, $"Harici komut silmek için (!haricikomutsil <harici_komut_adı>) şeklinde giriş yapınız. Örneğin; !haricikomutsil dc");
                return;
            }

            string feedback = String.Empty;
            if(externalCommandList.Contains($"!{parts[1]}"))
            {
                externalCommandList.Remove($"!{parts[1]}");
                feedback = $"Bu (!{parts[1]}) komut başarıyla silindi.";
            }
            else feedback = $"Böyle (!{parts[1]}) bir komut bulunmuyor.";

            client.SendMessage(e.ChatMessage.Channel, feedback);
            externalCommandList.SaveList(channelName, "ExternalCommandsList");
        }

        private void CommandKim(OnMessageReceivedArgs e, string[] parts)
        {
            if (parts.Length <= 1)
            {
                client.SendMessage(e.ChatMessage.Channel,
                $"Kim komudunu kullandıktan sonra bir isim yazmanız gerekmektedir. Örneğin; !kim rudyatkinson (@ kullanmanıza gerek yok!)");
                return;
            }

            string search = parts[1].ToLowerInvariant();
            if (userDictionary.ContainsKey(search)) client.SendMessage(e.ChatMessage.Channel, $"{search} --> {userDictionary[search]}");
            else client.SendMessage(e.ChatMessage.Channel,
            $"{parts[1]} henüz kendisini tanıtmamış. !ben yazarak kendini nasıl tanıtacağını öğrenebilir.");
        }

        private void CommandBen(OnMessageReceivedArgs e, string[] parts)
        {
            if (parts.Length <= 1)
            {
                client.SendMessage(channelName,
                $"Ben komudunu kullandıktan sonra kendinizi tanıtmanız gerekmektedir. Başkası için tanıtım yazamazsınız. Örneğin; !ben Yazılım geliştirme ve oyun tasarımı üzerine çalışıyorum.");
                return;
            }

            if (!userDictionary.ContainsKey(e.ChatMessage.Username)) userDictionary.Add(e.ChatMessage.Username, String.Empty);
            userDictionary[e.ChatMessage.Username] = String.Empty;

            for (int i = 1; i < parts.Length; i++)
                userDictionary[e.ChatMessage.Username] += $"{parts[i]} ";

            userDictionary.SaveDictionary(channelName, "UserDictionary");
        }

        private void CommandKomutlar(OnMessageReceivedArgs e)
        {
            string output = "Kullanıcı Komutları -->";

            output += externalCommandList.AddToString(null);
            output += commandList.AddToString(externalCommandList);
            output += commandDictionary.Keys.ToList().AddToString(commandList);

            if (IsValidToModerate(e.ChatMessage) && modCommandList.Count > 0) 
            output += $"\n|\nMod Komutları --> {modCommandList.AddToString(null)}";

            client.SendMessage(e.ChatMessage.Channel, output);
        }

        public void Chat(string s)
        {
            client.SendMessage(channelName, s);
        }

        public string GetBotName { get { return userName; } }

        public bool IsValidToModerate(ChatMessage chatMessage)
        {
            return chatMessage.IsBroadcaster || chatMessage.IsModerator || chatMessage.Username.Equals(author);
        }

    }
}
