using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace Rodkulman.Telegram
{
    public class DailyMessageProcessor
    {
        private readonly Timer timer;

        private DayOfWeek goodMorningMessageLastSent = DayOfWeek.Sunday;        
        private bool wednesdayMessageSent = false;
        private bool thursdayMessageSent = false;
    
        public DailyMessageProcessor()
        {
            timer = new Timer(TimerTick, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
        }
        
        private async void TimerTick(object state)
        {
            if (DateTime.Now.Hour < 7) { return; }

            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Wednesday:
                    if (!wednesdayMessageSent)
                    {
                        wednesdayMessageSent = true;
                        foreach (var id in DB.Chats)
                        {
                            await SendWednesdayMessage(id);
                        }
                    }
                    break;
                case DayOfWeek.Thursday:
                    if (!thursdayMessageSent)
                    {
                        thursdayMessageSent = true;
                        foreach (var id in DB.Chats)
                        {
                            await SendThurdayMessage(id);
                        }
                    }
                    break;
                default:
                    wednesdayMessageSent = false;
                    thursdayMessageSent = false;

                    if (DateTime.Now.DayOfWeek != goodMorningMessageLastSent)
                    {
                        goodMorningMessageLastSent = DateTime.Now.DayOfWeek;
                        foreach (var id in DB.Chats)
                        {
                            await GoogleImages.SendRandomImage(id, "bom+dia");
                        }
                    }
                    break;
            }
        }

        public async Task SendWednesdayMessage(long chatId)
        {
            await Program.Bot.SendChatActionAsync(chatId, ChatAction.UploadPhoto);

            var filePath = Resources.GetImages().Where(x => x.StartsWith("wednesday")).GetRandomElement();            

            using (var stream = Resources.GetFile(filePath))
            {
                await Program.Bot.SendStickerAsync(chatId, stream);
            }
        }

        public async Task SendThurdayMessage(long chatId)
        {
            await Program.Bot.SendChatActionAsync(chatId, ChatAction.Typing);
            await Program.Bot.SendTextMessageAsync(chatId, Resources.GetString("Thursday"));

            await Program.Bot.SendChatActionAsync(chatId, ChatAction.UploadAudio);
            using (var stream = Resources.GetAudio("thursday.acc"))
            {
                await Program.Bot.SendAudioAsync(chatId, stream);
            }
        }
    }
}