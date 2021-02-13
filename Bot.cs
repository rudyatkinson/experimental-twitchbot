using System;
using System.Collections.Generic;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using System.Linq;



namespace Granzwelt.TwitchBot
{
    public class Bot
    {
        #region attributes

        public const string author = "rudyatkinson";
        public string botName { get; private set; }
        public string accessToken { get; private set; }
        public string channelName { get; private set; }

        #endregion

        #region runtime editable collections

        public Dictionary<string, string> userDictionary { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> commandDictionary { get; private set; } = new Dictionary<string, string>();
        public List<string> externalCommandList { get; private set; } = new List<string>();
        public Dictionary<string, List<string>> listDictionary { get; private set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, int> counterDictionary { get; private set; } = new Dictionary<string, int>();
        public Dictionary<string, TimedCommand> timedCommandDictionary { get; private set; } = new Dictionary<string, TimedCommand>();

        #endregion

        #region exclusive commands lists

        List<string> commandList = new List<string>()
        {
            "!komutlar",
            "!ben",
            "!kim",
            "!sarki",
            "!sayaclar"
        };

        List<string> modCommandList = new List<string>()
        {
            "!haricikomutekle",
            "!haricikomutsil",
            "!komutekle",
            "!komutsil",
            "!listeolustur",
            "!listesil",
            "!sayacolustur",
            "!sayacsil",
            "!dongumesaj",
            "!dongumesajsil",
            "!dongumesajlar"
        };

        #endregion

        public TwitchClient client { get; private set; }

        public Bot()
        {
            if (!GetSettings())
            {
                Console.Clear();
                Console.WriteLine("Lütfen Twitch için config ayarlarını yapınız.\n" +
                Settings.filePath +
                "\n\nÇıkış yapmak için bir tuşa basınız...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        public void Connect()
        {

            ConnectionCredentials credentials = new ConnectionCredentials(botName, accessToken);

            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            var customClient = new WebSocketClient(clientOptions);

            client = new TwitchClient(customClient);
            client.Initialize(credentials, channelName, '!');

            client.OnConnected += Client_OnConnected;
            client.OnMessageReceived += Client_OnMessageReceived;

            client.Connect();

            Client_LoadContents();

        }

        public void Deneme()
        {
            Chat("thread denemesi");
        }

        private void Client_LoadContents()
        {
            userDictionary.LoadDictionary(channelName, "UserDictionary");
            commandDictionary.LoadDictionary(channelName, "CommandDictionary");
            externalCommandList.LoadList(channelName, "ExternalCommandsList");
            List<string> listElements = new List<string>();
            listElements.LoadList(channelName, "ListDictionaryAsList");
            if (listElements.Count > 0)
            {
                foreach (string s in listElements)
                {
                    listDictionary.Add(s, new List<string>());
                    listDictionary[s].LoadList(channelName, s);
                }
            }
            counterDictionary.LoadDictionary(channelName, "CounterDictionary");
            timedCommandDictionary.LoadTimedMessages(channelName, "TimedCommands", this);
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            client.SendMessage(e.AutoJoinChannel, $"{botName} kanala bağlandı! [ref --> github.com/TwitchLib, version --> 0.2, author --> @RudyAtkinson] !komutlar");
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
                        CommandKim(parts);
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
                        CommandSarki();
                        break;
                    case "!listeolustur":
                        CommandListeOlustur(e, parts);
                        break;
                    case "!listesil":
                        CommandListeSil(e, parts);
                        break;
                    case "!listeler":
                        CommandListeler(e);
                        break;
                    case "!sayacolustur":
                        CommandSayacEkle(e, parts);
                        break;
                    case "!sayacsil":
                        CommandSayacSil(e, parts);
                        break;
                    case "!sayaclar":
                        CommandSayaclar(e);
                        break;
                    case "!dongumesaj":
                        CommandZamanlayici(e, parts);
                        break;
                    case "!dongumesajsil":
                        CommandZamanlayiciSil(e, parts);
                        break;
                    case "!dongumesajlar":
                        CommandZamanlayicilar(e);
                        break;
                    default:
                        string feedback = String.Empty;
                        if (commandDictionary.ContainsKey($"{parts[0]}")) feedback += $" {commandDictionary[$"{parts[0]}"]}";
                        if (listDictionary.Keys.Contains(parts[0].Replace('!', ' ').Trim())) feedback += $" {CommandListActivity(e, parts)}";
                        if (counterDictionary.ContainsKey(parts[0].Replace('!', ' ').Trim())) feedback += $" {CommandSayacActivity(e, parts)}";

                        if (!String.IsNullOrEmpty(feedback)) Chat(feedback.Trim());
                        break;
                }
            }
        }

        private void CommandZamanlayici(OnMessageReceivedArgs e, string[] parts)
        {
            if (!IsValidToModerate(e.ChatMessage)) return;

            int duration = -1;
            switch (parts.Length)
            {
                case 1:

                    Chat($"Geçerli komut: !dongumesaj <mesaj_adi> <döngü_süresi_dakika> <döngü mesajı>");
                    break;

                case 2:

                    if (timedCommandDictionary.ContainsKey(parts[1]))
                    {
                        TimedCommand tempCommand = timedCommandDictionary[parts[1]];
                        Chat($"{tempCommand.commandName} adlı mesaj döngü süresi: {tempCommand.duration} dakika, dönen mesaj: {tempCommand.feedback}");
                    }
                    else Chat($"{parts[1]} adlı döngü mesaj oluşturulmamış.");

                    break;

                case 3:

                    if (!timedCommandDictionary.ContainsKey(parts[1]))
                    {
                        Chat($"{parts[1]} adlı döngü mesaj oluşturulmamış.");
                        break;
                    }

                    if (int.TryParse(parts[2], out duration) && 
                    timedCommandDictionary.ContainsKey(parts[1])) timedCommandDictionary[parts[1]].ChangeDuration(duration);
                    else if (timedCommandDictionary.ContainsKey(parts[1])) timedCommandDictionary[parts[1]].ChangeFeedback(parts[2]);
                    timedCommandDictionary.SaveTimedMessages(channelName, "TimedCommands");
                    break;

                default:

                    if (!int.TryParse(parts[2], out duration))
                    {
                        if (!timedCommandDictionary.ContainsKey(parts[1]))
                        {
                            Chat($"{parts[1]} adlı döngü mesaj oluşturulmamış.");
                            break;
                        }

                        string newFeedback = String.Empty;
                        for (int i = 2; i < parts.Length; i++)
                        {
                            newFeedback += parts[i];
                            if (i != parts.Length - 1) newFeedback += ' ';
                        }

                        timedCommandDictionary[parts[1]].ChangeFeedback(newFeedback);
                        timedCommandDictionary.SaveTimedMessages(channelName, "TimedCommands");
                        break;
                    }

                    if (timedCommandDictionary.ContainsKey(parts[1]))
                    {
                        Chat($"{parts[1]} adlı döngü mesaj zaten var.");
                        break;
                    }

                    string feedback = String.Empty;
                    for (int i = 3; i < parts.Length; i++)
                    {
                        feedback += parts[i];
                        if (i != parts.Length - 1) feedback += ' ';
                    }
                    timedCommandDictionary.Add(parts[1], new TimedCommand(this, parts[1], feedback, duration));
                    timedCommandDictionary.SaveTimedMessages(channelName, "TimedCommands");
                    break;

            }
        }

        private void CommandZamanlayiciSil(OnMessageReceivedArgs e, string[] parts)
        {
            if (!IsValidToModerate(e.ChatMessage)) return;

            if (parts.Length <= 1)
            {
                Chat("Geçerli Komut: !dongumesajsil <döngü_adi>");
                return;
            }

            if (timedCommandDictionary.ContainsKey(parts[1]))
            {
                timedCommandDictionary[parts[1]].DisableTimedCommand();
                timedCommandDictionary.Remove(parts[1]);
                timedCommandDictionary.SaveTimedMessages(channelName, "TimedCommands");
                Chat($"{parts[1]} isimli döngü mesaj silindi.");
            }
            else
            {
                Chat($"Böyle {parts[1]} bir döngü mesaj zaten bulunmuyor.");
            }
        }

        private void CommandZamanlayicilar(OnMessageReceivedArgs e)
        {
            if (!IsValidToModerate(e.ChatMessage)) return;

            if (timedCommandDictionary.Count > 0)
            {
                string feedback = "Döngü Mesajlar --> ";
                int index = 0;
                foreach (string s in timedCommandDictionary.Keys)
                {
                    feedback += s;

                    if (++index != timedCommandDictionary.Count) feedback += ", ";
                }
                Chat(feedback);
            }
            else
            {
                Chat("Oluşturulmuş döngü mesaj bulunamadı.");
            }


        }

        private void CommandKomutEkle(OnMessageReceivedArgs e, string[] parts)
        {
            if (!IsValidToModerate(e.ChatMessage)) return;

            if (parts.Length <= 2)
            {
                Chat($"Komut eklemek için (!komutekle <komut_adı> <komut açıklaması>) şeklinde giriş yapılmalıdır. Örneğin; !komutekle okul ağğğbi boğaziçi");
                return;
            }

            if (commandList.Contains($"!{parts[1]}") ||
            modCommandList.Contains($"!{parts[1]}"))
            {
                Chat($"Komut (!{parts[1]}) zaten mevcut.");
                return;
            }

            if (!commandDictionary.ContainsKey($"!{parts[1]}"))
            {
                commandDictionary.Add($"!{parts[1]}", null);
                Chat($"Komut (!{parts[1]}) eklendi.");
            }
            else Chat($"Komut (!{parts[1]}) güncellendi.");

            string temp = String.Empty;
            for (int i = 2; i < parts.Length; i++) temp += $"{parts[i]} ";

            if (externalCommandList.Contains($"!{parts[1]}")) externalCommandList.Remove($"!{parts[1]}");

            commandDictionary[$"!{parts[1]}"] = temp;
            commandDictionary.SaveDictionary(channelName, "CommandDictionary");
        }

        private void CommandKomutSil(OnMessageReceivedArgs e, string[] parts)
        {
            if (!IsValidToModerate(e.ChatMessage)) return;

            if (parts.Length <= 1)
            {
                Chat($"Komut silmek için (!komutsil <komut_adı>) şeklinde giriş yapılmalıdır. Örneğin; !komutsil okul");
                return;
            }

            string feedback = String.Empty;
            if (commandDictionary.ContainsKey($"!{parts[1]}"))
            {
                commandDictionary.Remove($"!{parts[1]}");
                feedback = $"Bu (!{parts[1]}) komut başarıyla silindi.";
            }
            else feedback = $"Böyle (!{parts[1]}) bir komut bulunmuyor.";

            Chat(feedback);
            commandDictionary.SaveDictionary(channelName, "CommandDictionary");
        }

        private void CommandSarki() => Chat(ProcessInfo.GetSpotifyInfo());

        private void CommandHariciKomutEkle(OnMessageReceivedArgs e, string[] parts)
        {
            if (!IsValidToModerate(e.ChatMessage)) return;

            if (parts.Length <= 1)
            {
                Chat($"Harici komut, diğer botlarda zaten varolan komutları bu komut listesine eklemek için kullanılır. Kullanımı (!haricikomutekle <harici_komut_adı>) şeklinde yapılır. Örneğin; !haricikomutekle dc");
                return;
            }

            if (!externalCommandList.Contains($"!{parts[1]}") &&
            !commandList.Contains($"!{parts[1]}") &&
            !commandDictionary.ContainsKey($"!{parts[1]}") &&
            !modCommandList.Contains($"!{parts[1]}")) externalCommandList.Add($"!{parts[1]}");
            else
            {
                Chat($"Bu (!{parts[1]}) komut zaten bulunmaktadır.");
                return;
            }

            externalCommandList.SaveList(channelName, "ExternalCommandsList");
            Chat($"Başka bir botta kullanılan bu ({parts[1]}) komut, komutlar listesine eklendi.");
        }

        private void CommandHariciKomutSil(OnMessageReceivedArgs e, string[] parts)
        {
            if (!IsValidToModerate(e.ChatMessage)) return;

            if (parts.Length <= 1)
            {
                Chat($"Harici komut silmek için (!haricikomutsil <harici_komut_adı>) şeklinde giriş yapınız. Örneğin; !haricikomutsil dc");
                return;
            }

            string feedback = String.Empty;
            if (externalCommandList.Contains($"!{parts[1]}"))
            {
                externalCommandList.Remove($"!{parts[1]}");
                feedback = $"Bu (!{parts[1]}) komut başarıyla silindi.";
            }
            else feedback = $"Böyle (!{parts[1]}) bir komut bulunmuyor.";

            Chat(feedback);
            externalCommandList.SaveList(channelName, "ExternalCommandsList");
        }

        private void CommandKim(string[] parts)
        {
            if (parts.Length <= 1)
            {
                Chat($"Kim komudunu kullandıktan sonra bir isim yazmanız gerekmektedir. Örneğin; !kim rudyatkinson (@ kullanmanıza gerek yok!)");
                return;
            }

            string search = parts[1].ToLowerInvariant();
            if (userDictionary.ContainsKey(search)) Chat($"{search} --> {userDictionary[search]}");
            else Chat($"{parts[1]} henüz kendisini tanıtmamış. !ben yazarak kendini nasıl tanıtacağını öğrenebilir.");
        }

        private void CommandBen(OnMessageReceivedArgs e, string[] parts)
        {
            if (parts.Length <= 1)
            {
                Chat($"Ben komudunu kullandıktan sonra kendinizi tanıtmanız gerekmektedir. Başkası için tanıtım yazamazsınız. Örneğin; !ben Yazılım geliştirme ve oyun tasarımı üzerine çalışıyorum.");
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

            Chat(output);
        }

        private void CommandListeOlustur(OnMessageReceivedArgs e, string[] parts)
        {
            if (!IsValidToModerate(e.ChatMessage)) return;

            if (parts.Length <= 1)
            {
                Chat("Liste oluşturmak için (!listeolustur <liste_adı>) şeklinde giriş yapılmalıdır.");
                return;
            }

            if (!listDictionary.ContainsKey(parts[1]))
            {
                listDictionary.Add(parts[1], new List<string>());
                listDictionary[parts[1]].SaveList(e.ChatMessage.Channel, parts[1]);
                listDictionary.Keys.ToList().SaveList(e.ChatMessage.Channel, "ListDictionaryAsList");
                Chat($"{parts[1]} isimli liste oluşturuldu.");
            }
            else Chat($"Böyle ({parts[1]}) bir mesaj zaten var.");


        }

        private void CommandListeSil(OnMessageReceivedArgs e, string[] parts)
        {
            if (!IsValidToModerate(e.ChatMessage)) return;

            if (parts.Length <= 1)
            {
                Chat("Liste silmek için (!listesil <liste_adı>) şeklinde giriş yapılmalıdır.");
                return;
            }

            if (listDictionary.ContainsKey(parts[1]))
            {
                Chat($"{parts[1]} isimli liste başarıyla silindi.");
                listDictionary.Remove(parts[1]);
                SaveSystem.RemoveFile(e.ChatMessage.Channel, parts[1]);
                listDictionary.Keys.ToList().SaveList(e.ChatMessage.Channel, "ListDictionaryAsList");
            }
            else Chat($"Böyle ({parts[1]}) bir liste zaten bulunmuyor.");
        }

        private void CommandListeler(OnMessageReceivedArgs e)
        {
            string feedback = String.Empty;
            int index = 0;
            if (listDictionary.Count > 0)
            {
                foreach (string s in listDictionary.Keys)
                {
                    feedback += s;
                    if (index++ != listDictionary.Count - 1) feedback += ", ";
                }
            }
            if (!String.IsNullOrEmpty(feedback)) Chat(feedback);
        }

        private string CommandListActivity(OnMessageReceivedArgs e, string[] parts)
        {
            string title = parts[0].Replace('!', ' ').Trim();
            string feedback = String.Empty;
            if (parts.Length <= 1 && listDictionary[title].Count > 0)
            {
                int index = 0;
                foreach (string s in listDictionary[title])
                {
                    feedback += s;
                    if (index++ != listDictionary[title].Count - 1) feedback += ", ";
                }
                //if (!String.IsNullOrEmpty(feedback)) Chat(feedback);
            }
            else if (parts.Length > 1)
            {
                if (parts[1] == "ekle" && parts.Length >= 3)
                {
                    string result = String.Empty;
                    for (int i = 2; i < parts.Length; i++)
                    {
                        result += parts[i];
                        if (i != parts.Length - 1) result += " ";
                    }
                    listDictionary[title].Add(result);
                    feedback = $"{title} listesine ({result}) eklendi.";
                    listDictionary[title].SaveList(e.ChatMessage.Channel, title);
                }
                else if (parts[1] == "sil" && parts.Length >= 3)
                {
                    string result = String.Empty;
                    for (int i = 2; i < parts.Length; i++)
                    {
                        result += parts[i];
                        if (i != parts.Length - 1) result += " ";
                    }
                    listDictionary[title].Remove(result);
                    feedback = $"{title} listesinden ({result}) silindi.";
                    listDictionary[title].SaveList(e.ChatMessage.Channel, title);
                }
            }
            else feedback = $"{title} adlı liste boş.";

            return feedback;
        }

        private void CommandSayacEkle(OnMessageReceivedArgs e, string[] parts)
        {
            if (!IsValidToModerate(e.ChatMessage)) return;

            string feedback = String.Empty;
            if (parts.Length > 1)
            {
                if (!counterDictionary.ContainsKey(parts[1]))
                {
                    counterDictionary.Add(parts[1], 0);
                    feedback = $"{parts[1]} adlı sayaç oluşturuldu.";
                    counterDictionary.SaveDictionary(channelName, "CounterDictionary");
                }
                else feedback = $"Böyle ({parts[1]}) bir sayaç zaten var.";
            }
            else feedback = $"Sayaç eklemek için !sayacekle <sayac_adi> şeklinde giriş yapılmalıdır.";

            Chat(feedback);
        }

        private void CommandSayacSil(OnMessageReceivedArgs e, string[] parts)
        {
            if (!IsValidToModerate(e.ChatMessage)) return;

            string feedback = String.Empty;
            if (parts.Length > 1)
            {
                if (counterDictionary.ContainsKey(parts[1]))
                {
                    counterDictionary.Remove(parts[1]);
                    feedback = $"{parts[1]} adlı sayaç silindi.";
                    counterDictionary.SaveDictionary(channelName, "CounterDictionary");
                }
                else feedback = $"Böyle ({parts[1]}) bir sayaç zaten bulunmuyor.";
            }
            else feedback = $"Sayaç silmek için !sayacsil <sayac_adi> şeklinde giriş yapılmalıdır.";

            Chat(feedback);
        }

        private void CommandSayaclar(OnMessageReceivedArgs e)
        {
            string feedback = String.Empty;
            int index = 0;

            foreach (string s in counterDictionary.Keys)
            {
                feedback += s;
                if (index != counterDictionary.Count - 1) feedback += ", ";
            }

            if (index < 1) feedback = $"Oluşturulmuş sayaç bulunamadı.";

            if (!String.IsNullOrEmpty(feedback)) Chat(feedback);
        }

        private string CommandSayacActivity(OnMessageReceivedArgs e, string[] parts)
        {
            string title = parts[0].Replace('!', ' ').Trim();
            string feedback = String.Empty;

            if (counterDictionary.ContainsKey(title))
            {
                feedback = $"{title}: {++counterDictionary[title]}";
                counterDictionary.SaveDictionary(channelName, "CounterDictionary");
            }

            return feedback;
        }

        private bool GetSettings()
        {
            string[] settings = Settings.GetConfig(Settings.ConfigFile.Twitch);
            if (settings == null) return false;

            int index = 0;
            foreach (string s in settings)
            {
                string[] parts = s.Split('=');
                if (parts.Length > 1)
                {
                    settings[index++] = parts[1].Trim();
                }
                else settings[index++] = string.Empty;
            }
            if (String.IsNullOrEmpty(settings[0]) ||
            String.IsNullOrEmpty(settings[1]) ||
            String.IsNullOrEmpty(settings[2])) return false;

            channelName = settings[0];
            botName = settings[1];
            accessToken = settings[2];

            return true;
        }

        public void Chat(string s)
        {
            client.SendMessage(channelName, s);
        }

        public string GetBotName { get { return botName; } }

        public bool IsValidToModerate(ChatMessage chatMessage)
        {
            return chatMessage.IsBroadcaster || chatMessage.IsModerator || chatMessage.Username.Equals(author);
        }

    }
}