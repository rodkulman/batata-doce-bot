using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net;
using Newtonsoft.Json;
using System.Text;

namespace Rodkulman.Telegram
{
    public class Program
    {
        internal static TelegramBotClient Bot;
        private static Timer tm;
        private static readonly List<long> chatIds = new List<long>();
        private static readonly Random rnd = new Random();
        private static User me;

        public static void Main(string[] args)
        {
            chatIds.AddRange((GoogleCloudStorage.GetArrayFromFile("db/chats.json")).Result.Select(x => x.Value<long>()));

            Bot = new TelegramBotClient(Keys.Get("Telegram"));
            tm = new Timer(TimerTick, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQuery;
            Bot.OnReceiveError += BotOnReceiveError;

            me = Bot.GetMeAsync().Result;

            Bot.StartReceiving();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            Bot.StopReceiving();
        }

        private static async void BotOnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            try
            {
                await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
                await WhatIsCommand.ReplyMessage(e.CallbackQuery.Message, e.CallbackQuery);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static async void TimerTick(object state)
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Thursday)
            {
                if (DateTime.Now.Hour >= 7 && !DB.ThursdayMessageSent)
                {
                    DB.ThursdayMessageSent = true;
                    foreach (var id in chatIds)
                    {
                        await SendThurdayMessage(id);
                    }
                }
            }
            else if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
            {
                if (DateTime.Now.Hour >= 7 && !DB.WednesdayMyDudes)
                {
                    DB.WednesdayMyDudes = true;
                    foreach (var id in chatIds)
                    {
                        await SendRandomImageMessage(id, "images/wednesday", bamboozle: false);
                    }
                }
            }
            else if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
            {
                if (DateTime.Now.Hour >= 7 && !DB.GottaGetDownOnFriday)
                {
                    DB.GottaGetDownOnFriday = true;
                    foreach (var id in chatIds)
                    {
                        await SendFridayLink(id);
                    }
                }
            }
            else
            {
                DB.ThursdayMessageSent = false;
                DB.WednesdayMyDudes = false;
                DB.GottaGetDownOnFriday = false;

                if (DateTime.Now.DayOfWeek != DB.GoodMorningMessageLastSent && DateTime.Now.Hour >= 7)
                {
                    DB.GoodMorningMessageLastSent = DateTime.Now.DayOfWeek;
                    foreach (var id in chatIds)
                    {
                        await GoogleImages.SendRandomImage(id, "bom+dia");
                    }
                }
            }
        }

        private static async Task SendThurdayMessage(long chatId)
        {
            await Bot.SendChatActionAsync(chatId, ChatAction.Typing);
            await Bot.SendTextMessageAsync(chatId, await GoogleCloudStorage.ReadAllText("text-replies/thursday.txt"), replyMarkup: new ReplyKeyboardRemove());

            await Bot.SendChatActionAsync(chatId, ChatAction.UploadAudio);
            using (var stream = await GoogleCloudStorage.GetFile("audio/thursday.aac"))
            {
                await Bot.SendAudioAsync(chatId, stream, replyMarkup: new ReplyKeyboardRemove());
            }
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null) { return; }

            if (message.Type != MessageType.Text) { return; }

            if (Reddit.ContainsSubredditMention(message.Text))
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                await Reddit.SendSneakPeek(message);
            }

            if (!message.Text.StartsWith("/"))
            {
                if (WhatIsCommand.IsExpected(message))
                {
                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                    await WhatIsCommand.ReplyMessage(message);
                }
                else
                {
                    await ReplyRandomMessage(message);
                }

                return;
            }

            // é uma menção de subreddit, não deve fazer nenhum comando
            if (message.Text.StartsWith("/r/", StringComparison.OrdinalIgnoreCase)) { return; }

            var command = message.Text.Split(' ').First().ToLower();
            var wasMentioned = false;

            if (command.Contains("@"))
            {
                if (command.Substring(command.IndexOf("@") + 1).Equals(me.Username))
                {
                    command = command.Substring(0, command.IndexOf("@"));
                    wasMentioned = true;
                }
                else
                {
                    return;
                }
            }

            switch (command)
            {
                case "/whatis":
                    await WhatIsCommand.ReplyMessage(message);
                    break;
                case "/top":
                    await SendTopMessage(message);
                    break;
                case "/wednesday":
                    await SendRandomImageMessage(message.Chat.Id, "images/wednesday", message.MessageId);
                    break;
                case "/thursday":
                    await SendThurdayMessage(message.Chat.Id);
                    break;
                case "/friday":
                    await SendFridayLink(message.Chat.Id);
                    break;
                case "/communism":
                    await SendRandomImageMessage(message.Chat.Id, "images/communism", message.MessageId);
                    break;
                case "/jesus":
                    await SendRandomImageMessage(message.Chat.Id, "images/jesus", message.MessageId);
                    break;
                case "/ghandi":
                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Se escreve Gandhi", replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "/gandhi":
                    await SendRandomImageMessage(message.Chat.Id, "images/gandhi", message.MessageId);
                    break;
                case "/bomdia":
                    await GoogleImages.SendRandomImage(message.Chat.Id, "bom+dia");
                    break;
                case "/start":
                    await SaveChat(message.Chat.Id);
                    await Bot.SendTextMessageAsync(message.Chat.Id, $"Que começe a zueira", replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "/stop":
                    await RemoveChat(message.Chat.Id);
                    await Bot.SendTextMessageAsync(message.Chat.Id, $"toma no cu vocês", replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "/roll":
                    await DiceRolls.SendRollDiceMessage(message);
                    break;
                case "/dankmeme":
                    await Reddit.GetRandomImage(message, (await GoogleCloudStorage.ReadAllLines("text-lists/meme-subreddit.txt")).GetRandomElement());
                    break;
                default:
                    if (message.Chat.Type == ChatType.Private || wasMentioned)
                    {
                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (message.From.Username == "rodkulman")
                        {
                            await Bot.SendTextMessageAsync(message.Chat.Id, $"Desculpa meu mestre, mas o comando que você quis não existe 😞", replyMarkup: new ReplyKeyboardRemove());
                        }
                        else
                        {
                            await Bot.SendTextMessageAsync(message.Chat.Id, $"Porra {message.From.FirstName}, eu ainda não aprendi a fazer essa merda.", replyMarkup: new ReplyKeyboardRemove());
                        }
                    }

                    break;
            }
        }

        private static async Task SendFridayLink(long chatId)
        {
            var link = (await GoogleCloudStorage.ReadAllLines("text-lists/friday-links.txt")).GetRandomElement();

            await Bot.SendTextMessageAsync(chatId, $"<a href=\"{link}\">sextou</a>", ParseMode.Html, disableWebPagePreview: true);
        }

        private static async Task SaveChat(long id)
        {
            if (!chatIds.Contains(id)) { chatIds.Add(id); }

            using (var mem = new System.IO.MemoryStream())
            {
                using (var textWriter = new System.IO.StreamWriter(mem, Encoding.UTF8, 1024, true))
                using (var jsonWriter = new JsonTextWriter(textWriter))
                {
                    await JArray.FromObject(chatIds).WriteToAsync(jsonWriter);
                }

                mem.Seek(0, System.IO.SeekOrigin.Begin);
                await GoogleCloudStorage.SetFile("db/chats.json", "text/plain", mem);
            }
        }

        private static async Task RemoveChat(long id)
        {
            if (chatIds.Contains(id)) { chatIds.Remove(id); }

            using (var mem = new System.IO.MemoryStream())
            {
                using (var textWriter = new System.IO.StreamWriter(mem))
                using (var jsonWriter = new JsonTextWriter(textWriter))
                {
                    await JArray.FromObject(chatIds).WriteToAsync(jsonWriter);
                }

                mem.Seek(0, System.IO.SeekOrigin.Begin);
                await GoogleCloudStorage.SetFile("db/chats.json", "text/plain", mem);
            }
        }

        private static async Task SendTopMessage(Message message)
        {
            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            await Bot.SendTextMessageAsync(message.Chat.Id, "https://twitter.com/neymarjr/status/19370237272", replyToMessageId: message.MessageId, replyMarkup: new ReplyKeyboardRemove());
        }

        private static async Task ReplyRandomMessage(Message message)
        {
            var communismKeywords = await GoogleCloudStorage.ReadAllLines("keywords/communism.txt");
            var topKeywords = await GoogleCloudStorage.ReadAllLines("keywords/top.txt");
            var jesusKeywords = await GoogleCloudStorage.ReadAllLines("keywords/jesus.txt");
            var bamboozleKeywords = await GoogleCloudStorage.ReadAllLines("keywords/bamboozle.txt");

            foreach (Match match in Regex.Matches(message.Text, @"\b.+?\b"))
            {
                if (topKeywords.Contains(match.Value, StringComparer.OrdinalIgnoreCase))
                {
                    await SendTopMessage(message);
                }

                if (match.Value.Equals("ghandi", StringComparison.OrdinalIgnoreCase))
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Se escreve Gandhi", replyToMessageId: message.MessageId, replyMarkup: new ReplyKeyboardRemove());
                }

                if (bamboozleKeywords.Contains(match.Value, StringComparer.OrdinalIgnoreCase))
                {
                    using (var mem = await GoogleCloudStorage.GetFile("images/bamboozle/walter.jpg"))
                    {
                        await Bot.SendPhotoAsync(message.Chat.Id, mem, "I am the one who bamboozles!", replyToMessageId: message.MessageId);                        
                    }                    
                }
            }

            foreach (Match match in Regex.Matches(message.Text, @"\b(.+?)\b\.(jpg|jpeg|bmp|png|gif)", RegexOptions.IgnoreCase))
            {
                var directImage = GoogleCloudStorage.GetFiles("images", recurse: true).FirstOrDefault(x => System.IO.Path.GetFileName(x).Equals(match.Value.Trim(), StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(directImage))
                {
                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                    using (var stream = await GoogleCloudStorage.GetFile(directImage))
                    {
                        if (match.Groups[2].Value.Equals("gif", StringComparison.OrdinalIgnoreCase))
                        {
                            await Bot.SendVideoAsync(message.Chat.Id, stream, replyToMessageId: message.MessageId);
                        }
                        else
                        {
                            await Bot.SendPhotoAsync(message.Chat.Id, stream, replyToMessageId: message.MessageId);
                        }
                    }
                }
                else
                {
                    if (jesusKeywords.Contains(match.Groups[1].Value, StringComparer.OrdinalIgnoreCase))
                    {
                        await SendRandomImageMessage(message.Chat.Id, "images/jesus", message.MessageId);
                    }

                    if (communismKeywords.Contains(match.Groups[1].Value, StringComparer.OrdinalIgnoreCase))
                    {
                        await SendRandomImageMessage(message.Chat.Id, "images/communism", message.MessageId);
                    }
                }
            }

            await Communism.CorrectCapitalistInfidels(message);
        }

        private static async Task SendRandomImageMessage(long chatId, string path, int messageId = 0, bool bamboozle = true)
        {
            if (bamboozle && rnd.Next(0, 10) == 5)
            {
                using (var stream = await GoogleCloudStorage.GetFile("images/rick.png"))
                {
                    await Bot.SendPhotoAsync(chatId, stream, replyToMessageId: messageId, caption: "bamboozled");
                }

                return;
            }

            await Bot.SendChatActionAsync(chatId, ChatAction.UploadPhoto);

            var files = GoogleCloudStorage.GetFiles(path);

            using (var stream = await GoogleCloudStorage.GetFile(files.GetRandomElement()))
            {
                await Bot.SendStickerAsync(chatId, stream, replyToMessageId: messageId);
            }
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }
    }
}
