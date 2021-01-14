using System.Collections.Generic;
using System.IO;

namespace TwitchBot.Granzwelt
{
    public static class SaveSystem
    {
        const string filePath = "Contents/";

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

        public static void SaveList(this List<string> list, string channel, string folderName)
        {
            if (!Directory.Exists($"{filePath}")) Directory.CreateDirectory($"{filePath}");

            using (StreamWriter writer = new StreamWriter($"{filePath}{folderName}_{channel}.txt"))
            {
                foreach (string s in list)
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
    }
}