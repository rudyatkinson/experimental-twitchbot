using System;
using System.Diagnostics;
using System.Linq;

namespace TwitchBot.Granzwelt
{
    public class ProcessInfo
    {
        public static string GetSpotifyInfo()
        {
            var process = Process.GetProcessesByName("Spotify")
            .FirstOrDefault( s => !string.IsNullOrWhiteSpace(s.MainWindowTitle));

            if(process == null) return "Spotify açık değil.";
            else if(string.Equals(process.MainWindowTitle, 
            "Spotify", 
            StringComparison.InvariantCultureIgnoreCase) || 
            string.Equals(process.MainWindowTitle, 
            "Spotify Free", 
            StringComparison.InvariantCultureIgnoreCase)) return "Spotify durdurulmuş.";
            
            return process.MainWindowTitle;
        }
    }
}