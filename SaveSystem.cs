using System.Collections.Generic;
using System.IO;
using System;

namespace TwitchBot.Granzwelt
{
    public static class SaveSystem
    {
        static string filePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Granzwelt/RudyAsBot/Content/";

        public static void SaveDictionary(this Dictionary<string, string> dictionary, string channel, string folderName)
        {
            if (!Directory.Exists($"{filePath}")) Directory.CreateDirectory($"{filePath}");

            using (StreamWriter writer = new StreamWriter($"{filePath}{folderName}_{channel}.csv"))
            {
                foreach (KeyValuePair<string, string> dict in dictionary)
                {
                    writer.WriteLine($"{dict.Key}; {dict.Value}");
                }
            }
        }

        public static void SaveDictionary(this Dictionary<string, int> dictionary, string channel, string folderName)
        {
            if (!Directory.Exists($"{filePath}")) Directory.CreateDirectory($"{filePath}");

            using (StreamWriter writer = new StreamWriter($"{filePath}{folderName}_{channel}.csv"))
            {
                foreach (KeyValuePair<string, int> dict in dictionary)
                {
                    writer.WriteLine($"{dict.Key}; {dict.Value}");
                }
            }
        }

        public static void SaveTimedMessages(this Dictionary<string, TimedCommand> dictionary, string channel, string folderName)
        {
            if (!Directory.Exists($"{filePath}")) Directory.CreateDirectory($"{filePath}");

            if(dictionary.Count <= 0)
            {
                EmptyFile(channel, folderName);
                return;
            }

            using (StreamWriter writer = new StreamWriter($"{filePath}{folderName}_{channel}.csv"))
            {
                foreach(KeyValuePair<string, TimedCommand> dict in dictionary)
                {
                    string data = String.Empty;
                    data += dict.Value.duration + ";";
                    data += dict.Value.feedback;
                    writer.WriteLine($"{dict.Key};{data}");
                }
            }
        }

        public static void LoadTimedMessages(this Dictionary<string, TimedCommand> dictionary, string channel, string folderName, Bot bot)
        {
            if (File.Exists($"{filePath}{folderName}_{channel}.csv"))
            {
                string[] parts;
                using (StreamReader reader = new StreamReader($"{filePath}{folderName}_{channel}.csv"))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        parts = line.Split(';');
                        dictionary.Add(parts[0], new TimedCommand(bot, parts[0], parts[2], int.Parse(parts[1])));
                    }
                }
            }
        }

        public static void LoadDictionary(this Dictionary<string, string> dictionary, string channel, string folderName)
        {
            if (File.Exists($"{filePath}{folderName}_{channel}.csv"))
            {
                string[] parts;
                using (StreamReader reader = new StreamReader($"{filePath}{folderName}_{channel}.csv"))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        parts = line.Split(';');
                        dictionary.Add(parts[0], string.Empty);

                        for (int i = 1; i < parts.Length; i++)
                        {
                            dictionary[parts[0]] += parts[i];
                        }
                    }
                }
            }
            else
            {
                if (folderName == "CommandDictionary") dictionary.Add("!rudy", "Her akşam bana format atıp fişimi çekiyor :'(");
                if (folderName == "UserDictionary") dictionary.Add("rudyatkinson", "Yazılım geliştiricisi ve oyun tasarımcısıyım. Bu botu da her akşam formatlıyorum :)");
            }
        }

        public static void LoadDictionary(this Dictionary<string, int> dictionary, string channel, string folderName)
        {
            if (File.Exists($"{filePath}{folderName}_{channel}.csv"))
            {
                string[] parts;
                using (StreamReader reader = new StreamReader($"{filePath}{folderName}_{channel}.csv"))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        parts = line.Split(';');
                        dictionary.Add(parts[0], 0);

                        for (int i = 1; i < parts.Length; i++)
                        {
                            dictionary[parts[0]] = Int32.Parse(parts[i]);
                        }
                    }
                }
            }
        }

        public static void SaveList<T>(this List<T> list, string channel, string folderName)
        {
            List<string> temp = list as List<string>;
            if (!Directory.Exists($"{filePath}")) Directory.CreateDirectory($"{filePath}");
            if (temp.Count <= 0) 
            {
                EmptyFile(channel, folderName);
                return;
            }

            using (StreamWriter writer = new StreamWriter($"{filePath}{folderName}_{channel}.txt"))
            {
                foreach (string s in temp)
                {
                    writer.WriteLine(s);
                }
            }
        }

        public static void LoadList(this List<string> list, string channel, string folderName)
        {
            if (File.Exists($"{filePath}{folderName}_{channel}.txt"))
            {
                using (StreamReader reader = new StreamReader($"{filePath}{folderName}_{channel}.txt"))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        list.Add(line);
                    }
                }
            }
        }

        public static void RemoveFile(string channel, string folderName)
        {
            string path = $"{filePath}{folderName}_{channel}.txt";
            if(File.Exists(path)) File.Delete(path);
        }

        public static void EmptyFile(string channel, string folderName)
        {
            string path = $"{filePath}{folderName}_{channel}.txt";
            if (File.Exists(path))
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.WriteLine(String.Empty);
                }
            }
        }
    }
}