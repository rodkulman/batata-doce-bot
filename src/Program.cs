﻿using System;
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

        public static void Main(string[] args)
        {
            var keys = JObject.Parse(IO.File.ReadAllText("keys.json"));
            chatIds.AddRange(JArray.Parse(IO.File.ReadAllText(@"db\chats.json")).Select(x => x.Value<long>()));

            Bot = new TelegramBotClient(keys["Telegram"].Value<string>());
            tm = new Timer(TimerTick, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));

            Bot.OnMessage += BotOnMessageReceived;
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
                        await SendRandomImageMessage(id, @"images\wednesday", bamboozle: false);
                    }
                }
            }
            else
            {
                DB.ThursdayMessageSent = false;
                DB.WednesdayMyDudes = false;

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
            await Bot.SendTextMessageAsync(chatId, IO.File.ReadAllText(@"text-replies\thursday.txt"), replyMarkup: new ReplyKeyboardRemove());

            await Bot.SendChatActionAsync(chatId, ChatAction.UploadAudio);
            using (var stream = IO.File.OpenRead(@"audio\thursday.aac"))
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

            switch (message.Text.Split(' ').First().ToLower())
            {
                case "/whatis":
                    await WhatIsCommand.ReplyMessage(message);
                    break;
                case "/top":
                    await SendTopMessage(message);
                    break;
                case "/wednesday":
                    await SendRandomImageMessage(message.Chat.Id, @"images\wednesday", message.MessageId);
                    break;
                case "/thursday":
                    await SendThurdayMessage(message.Chat.Id);
                    break;
                case "/communism":
                    await SendRandomImageMessage(message.Chat.Id, @"images\communism", message.MessageId);
                    break;
                case "/jesus":
                    await SendRandomImageMessage(message.Chat.Id, @"images\jesus", message.MessageId);
                    break;
                case "/ghandi":
                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Se escreve Gandhi", replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "/gandhi":
                    await SendRandomImageMessage(message.Chat.Id, @"images\gandhi", message.MessageId);
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
                default:
                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                    if (message.From.Username == "rodkulman")
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, $"Desculpa meu mestre, mas o comando que você quis não existe 😞", replyMarkup: new ReplyKeyboardRemove());
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, $"{message.From.FirstName} para de tentar me bugar, porra", replyMarkup: new ReplyKeyboardRemove());
                    }
                    
                    break;
            }
        }

        private static async Task SaveChat(long id)
        {
            if (!chatIds.Contains(id)) { chatIds.Add(id); }

            using (var stream = IO.File.OpenWrite(@"db\chats.json"))
            using (var textWriter = new IO.StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                await JArray.FromObject(chatIds).WriteToAsync(jsonWriter);
            }
        }

        private static async Task RemoveChat(long id)
        {
            if (chatIds.Contains(id)) { chatIds.Remove(id); }

            using (var stream = IO.File.OpenWrite(@"db\chats.json"))
            using (var textWriter = new IO.StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                await JArray.FromObject(chatIds).WriteToAsync(jsonWriter);
            }
        }

        private static async Task SendTopMessage(Message message)
        {
            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            await Bot.SendTextMessageAsync(message.Chat.Id, "https://twitter.com/neymarjr/status/19370237272", replyToMessageId: message.MessageId, replyMarkup: new ReplyKeyboardRemove());
        }

        private static async Task ReplyRandomMessage(Message message)
        {
            var communismKeywords = await IO.File.ReadAllLinesAsync(@"keywords\communism.txt");
            var topKeywords = await IO.File.ReadAllLinesAsync(@"keywords\top.txt");
            var jesusKeywords = await IO.File.ReadAllLinesAsync(@"keywords\jesus.txt");
            var bamboozleKeywords = await IO.File.ReadAllLinesAsync(@"keywords\bamboozle.txt");

            foreach (Match match in Regex.Matches(message.Text, @"\b.+?\b"))
            {
                if (communismKeywords.Contains(match.Value, StringComparer.OrdinalIgnoreCase))
                {
                    await SendRandomImageMessage(message.Chat.Id, @"images\communism", message.MessageId);
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
                    await SendRandomImageMessage(message.Chat.Id, @"images\jesus", message.MessageId);
                }

                if (bamboozleKeywords.Contains(match.Value, StringComparer.OrdinalIgnoreCase))
                {
                    await Bot.SendPhotoAsync(message.Chat.Id, IO.File.OpenRead(@"images\bamboozle\walter.jpg"), "I am the one who bamboozles!", replyToMessageId: message.MessageId);
                }
            }
        }

        private static async Task SendRandomImageMessage(long chatId, string path, int messageId = 0, bool bamboozle = true)
        {
            if (bamboozle && rnd.Next(0, 10) == 5)
            {
                await Bot.SendTextMessageAsync(chatId, "You have been bamboozled", replyToMessageId: messageId);
                return;
            }

            await Bot.SendChatActionAsync(chatId, ChatAction.UploadPhoto);

            var files = IO.Directory.GetFiles(path);

            using (var stream = System.IO.File.OpenRead(files.GetRandomElement()))
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
