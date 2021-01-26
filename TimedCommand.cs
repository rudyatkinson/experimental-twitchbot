using System;
using System.Timers;

namespace TwitchBot.Granzwelt
{
    public class TimedCommand
    {
        Timer timer = new Timer();
        Bot bot;
        public string commandName {get; private set;}
        public string feedback {get; private set;}
        public int duration {get; private set;}

        public TimedCommand(Bot bot, string commandName, string feedback, int duration)
        {
            this.bot = bot;
            this.commandName = commandName;
            this.feedback = feedback;
            this.duration = duration;

            timer.Elapsed += SendTwitchAsMessage;
            timer.Interval = duration * 60 * 1000;
            timer.Enabled = true;
        }

        private void SendTwitchAsMessage(object o, EventArgs e) => bot.Chat(feedback);

        public void ChangeDuration(int newDuration)
        {
            duration = newDuration;
            timer.Interval = duration * 60 * 1000;
            bot.Chat($"{commandName} isimli zamanlı komut döngü süresi {duration} dakika olarak değiştirildi.");
        }

        public void ChangeFeedback(string newFeedback)
        {
            feedback = newFeedback;
            bot.Chat($"{commandName} isimli zamanlı komut dönüşü değiştirildi");
        }
    }
}