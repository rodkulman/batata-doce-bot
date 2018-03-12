using System;
using System.Collections.Generic;
using IO = System.IO;
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

namespace Rodkulman.Telegram
{
    public class Program
    {
        internal static TelegramBotClient Bot;
        private static Timer tm;
        private static bool thursdayMessageSent = false;
        private static readonly List<long> chatIds = new List<long>();

        public static void Main(string[] args)
        {
            var keys = JObject.Parse(IO.File.ReadAllText("keys.json"));

            Bot = new TelegramBotClient(keys["Telegram"].Value<string>());
            tm = new Timer(TimerTick, null, TimeSpan.Zero, TimeSpan.FromHours(1));

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            var me = Bot.GetMeAsync().Result;

            Bot.StartReceiving();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            Bot.StopReceiving();
        }

        private static async void TimerTick(object state)
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Thursday)
            {
                if (DateTime.Now.Hour >= 8 && !thursdayMessageSent)
                {
                    thursdayMessageSent = true;
                    foreach (var id in chatIds)
                    {
                        await SendThurdayMessage(id);
                    }
                }
            }
            else
            {
                thursdayMessageSent = false;
            }
        }

        private static async Task SendThurdayMessage(long chatId)
        {
            await Bot.SendTextMessageAsync(chatId, IO.File.ReadAllText(@"text-replies\thursday.txt"), replyMarkup: new ReplyKeyboardRemove());
            using (var stream = IO.File.OpenRead(@"audio\thursday.aac"))
            {
                await Bot.SendAudioAsync(chatId, stream, replyMarkup: new ReplyKeyboardRemove());
            }
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (!chatIds.Contains(message.Chat.Id))
            {
                chatIds.Add(message.Chat.Id);
            }

            if (message == null || message.Type != MessageType.Text) { return; }

            if (!message.Text.StartsWith("/"))
            {
                if (WhatIsCommand.IsExpected(message))
                {
                    await WhatIsCommand.ReplyMessage(message);
                }
                else
                {
                    await ReplyRandomMessage(message);
                }

                return;
            }

            switch (message.Text.Split(' ').First().ToLower())
            {
                case "/whatis":
                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                    await WhatIsCommand.ReplyMessage(message);

                    break;
                case "/top":
                    await SendTopMessage(message);
                    break;
                case "/thursday":
                    await SendThurdayMessage(message.Chat.Id);
                    break;
                case "/communism":
                    await SendRandomImageMessage(message, @"images\communism");
                    break;
                case "/jesus":
                    await SendRandomImageMessage(message, @"images\jesus");
                    break;
                case "/ghandi":
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Se escreve Gandhi", replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "/gandhi":
                    await SendRandomImageMessage(message, @"images\gandhi");
                    break;
                default:
                    await Bot.SendTextMessageAsync(message.Chat.Id, "O que tu tentou fazer não é dank o suficiente", replyMarkup: new ReplyKeyboardRemove());
                    break;
            }
        }

        private static async Task SendTopMessage(Message message)
        {
            await Bot.SendTextMessageAsync(message.Chat.Id, "https://twitter.com/neymarjr/status/19370237272", replyToMessageId: message.MessageId, replyMarkup: new ReplyKeyboardRemove());
        }

        private static async Task ReplyRandomMessage(Message message)
        {
            var communismKeywords = await IO.File.ReadAllLinesAsync(@"keywords\communism.txt");
            var topKeywords = await IO.File.ReadAllLinesAsync(@"keywords\top.txt");
            var jesusKeywords = await IO.File.ReadAllLinesAsync(@"keywords\jesus.txt");

            foreach (Match match in Regex.Matches(message.Text, @"\b.+?\b"))
            {
                if (communismKeywords.Contains(match.Value, StringComparer.OrdinalIgnoreCase))
                {
                    await SendRandomImageMessage(message, @"images\communism");
                }

                if (topKeywords.Contains(match.Value, StringComparer.OrdinalIgnoreCase))
                {
                    await SendTopMessage(message);
                }

                if (match.Value.Equals("ghandi", StringComparison.OrdinalIgnoreCase))
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Se escreve Gandhi", replyToMessageId: message.MessageId, replyMarkup: new ReplyKeyboardRemove());
                }

                if (jesusKeywords.Contains(match.Value, StringComparer.OrdinalIgnoreCase))
                {
                    await SendRandomImageMessage(message, @"images\jesus");
                }
            }
        }

        private static async Task SendRandomImageMessage(Message message, string path)
        {
            var files = IO.Directory.GetFiles(path);

            using (var stream = System.IO.File.OpenRead(files.GetRandomElement()))
            {
                await Bot.SendStickerAsync(message.Chat.Id, stream, replyToMessageId: message.MessageId);
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await Bot.AnswerCallbackQueryAsync(
                callbackQueryEventArgs.CallbackQuery.Id,
                $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }
    }
}
