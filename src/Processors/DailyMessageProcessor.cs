using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Rodkulman.Telegram
{
    public class DailyMessageProcessor
    {
        private readonly Timer timer;
        private readonly WeatherManager weather;
        private readonly Random rnd;
        private DayOfWeek goodMorningMessageLastSent = DayOfWeek.Sunday;
        private DateTime stayHidratedMessageLastSent = DateTime.Today.AddDays(-1);
        private bool wednesdayMessageSent = false;
        private bool thursdayMessageSent = false;

        public DailyMessageProcessor()
        {
            weather = new WeatherManager();
            rnd = new Random();
            timer = new Timer(TimerTick, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        private async void TimerTick(object state)
        {
            if (stayHidratedMessageLastSent != DateTime.Today && DateTime.Now.Hour > 7 && rnd.Next(0, 10) == 0)
            {
                foreach (var id in DB.Chats)
                {
                    await Program.Bot.SendTextMessageAsync(id, Resources.GetString("StayHidratedMessage"));
                }

                stayHidratedMessageLastSent = DateTime.Today;
            }

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
                        var sendHotAudio = await weather.GetMaxTempForCity(WeatherManager.PortoAlegreId) >= 25 && await weather.GetMaxTempForCity(WeatherManager.TresDeMaioId) >= 25;

                        if (sendHotAudio)
                        {
                            using (var stream = Resources.GetAudio("gonna-be-hot.mp3"))
                            {
                                foreach (var id in DB.Chats)
                                {
                                    await Program.Bot.SendChatActionAsync(id, ChatAction.UploadAudio);
                                    await Program.Bot.SendAudioAsync(id, stream, title: "Pretty much everywhere", performer: "Arthur");
                                }
                            }
                        }
                        else
                        {
                            Stream image;

                            try
                            {
                                image = await GoogleImages.GetRandomImage("bom+dia");
                            }
                            catch
                            {
                                return;
                            }

                            using (image)
                            {
                                foreach (var id in DB.Chats)
                                {
                                    image.Position = 0;
                                    await Program.Bot.SendPhotoAsync(id, image);
                                }
                            }
                        }

                        goodMorningMessageLastSent = DateTime.Now.DayOfWeek;
                    }
                    break;
            }
        }

        public async Task SendWednesdayMessage(long chatId)
        {
            await Program.Bot.SendChatActionAsync(chatId, ChatAction.UploadPhoto);

            var filePath = Resources.GetImages().Where(x => Path.GetFileName(x).StartsWith("wednesday")).GetRandomElement();

            using (var stream = Resources.GetFile(filePath))
            {
                await Program.Bot.SendStickerAsync(chatId, stream);
            }
        }

        public bool IsThursdayReference(string message)
        {
            return Regex.IsMatch(message, "hoje ([eé]|eh) quinta", RegexOptions.IgnoreCase);
        }

        public async Task SendThurdayMessage(long chatId)
        {
            await Program.Bot.SendChatActionAsync(chatId, ChatAction.Typing);
            await Program.Bot.SendTextMessageAsync(chatId, Resources.GetString("Thursday"));

            await Program.Bot.SendChatActionAsync(chatId, ChatAction.UploadAudio);
            using (var stream = Resources.GetAudio("thursday.aac"))
            {
                await Program.Bot.SendAudioAsync(chatId, stream, title: "Dale Dale", performer: "Mito");
            }
        }
    }
}