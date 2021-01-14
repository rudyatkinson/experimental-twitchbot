using System;
using System.Collections.Generic;

namespace TwitchBot.Granzwelt
{
    public static class Extensions
    {
        public static string AddToString(this List<string> list, List<string> beforeList)
        {
            if(list.Count <= 0) return String.Empty;

            string result = String.Empty;
            int index = 0;

            if (beforeList != null && beforeList.Count > 0) result += ", ";
            foreach (var s in list)
            {
                result += $"{s}";
                index++;
                if (index < list.Count) result += ",\n";
            }

            return result;
        }
    }
}