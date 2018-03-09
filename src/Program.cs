using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
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
        public static readonly TelegramBotClient Bot = new TelegramBotClient("564117884:AAGwBXuL3v6AteHstPJ6_N3XdmWbN_BhBz8");

        public static void Main(string[] args)
        {
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            var me = Bot.GetMeAsync().Result;

            Bot.StartReceiving();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            Bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.Text) { return; }

            if (!message.Text.StartsWith("/"))
            {
                await ReplyRandomMessage(message);
                return;
            }

            switch (message.Text.Split(' ').First().ToLower())
            {
                case "/whatis":
                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                    foreach (var reply in WhatIsCommand.ReplyMessage(message.Text))
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, reply, replyMarkup: new ReplyKeyboardRemove());
                        await Task.Delay(1000);
                    }
                    break;
                case "/top":
                    await SendTopMessage(message);
                    break;
                case "/communism":
                    await SendCommunistPropaganda(message);
                    break;
                case "/jesus":
                    await SendJesusMessage(message);
                    break;
                case "/ghandi":
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Se escreve Gandhi", replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "/gandhi":
                    await SendGandhiMessage(message);
                    break;
                default:
                    await Bot.SendTextMessageAsync(message.Chat.Id, "O que tu tentou fazer não é dank o suficiente", replyMarkup: new ReplyKeyboardRemove());
                    break;
            }
        }

        private static async Task SendTopMessage(Message message)
        {
            await Bot.SendTextMessageAsync(message.Chat.Id, "https://twitter.com/neymarjr/status/19370237272", replyMarkup: new ReplyKeyboardRemove());
        }

        private static async Task ReplyRandomMessage(Message message)
        {
            if (Regex.IsMatch(message.Text, @"\b(russia|ussr|putin|comrade|jefer|communism|comunismo)\b", RegexOptions.IgnoreCase))
            {
                await SendCommunistPropaganda(message);
            }
            else if (Regex.IsMatch(message.Text, @"\btop\b", RegexOptions.IgnoreCase))
            {
                await SendTopMessage(message);
            }
            else if (Regex.IsMatch(message.Text, @"\bghandi\b", RegexOptions.IgnoreCase))
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, "Se escreve Gandhi", replyMarkup: new ReplyKeyboardRemove());
            }
            else if (Regex.IsMatch(message.Text, @"\bjesus\b", RegexOptions.IgnoreCase))
            {
                await SendJesusMessage(message);
            }
        }

        private static async Task SendJesusMessage(Message message)
        {
            var files = Directory.GetFiles(@"images\jesus");

            using (var stream = System.IO.File.OpenRead(files.GetRandomElement()))
            {
                await Bot.SendPhotoAsync(message.Chat.Id, stream);
            }
        }

        private static async Task SendGandhiMessage(Message message)
        {
            var files = Directory.GetFiles(@"images\gandhi");

            using (var stream = System.IO.File.OpenRead(files.GetRandomElement()))
            {
                await Bot.SendPhotoAsync(message.Chat.Id, stream);
            }
        }

        private static async Task SendCommunistPropaganda(Message message)
        {
            var files = Directory.GetFiles(@"images\communism");

            using (var stream = System.IO.File.OpenRead(files.GetRandomElement()))
            {
                await Bot.SendPhotoAsync(message.Chat.Id, stream);
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await Bot.AnswerCallbackQueryAsync(
                callbackQueryEventArgs.CallbackQuery.Id,
                $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }

        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            Console.WriteLine($"Received inline query from: {inlineQueryEventArgs.InlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                new InlineQueryResultLocation(
                    id: "1",
                    latitude: 40.7058316f,
                    longitude: -74.2581888f,
                    title: "New York")   // displayed result
                    {
                        InputMessageContent = new InputLocationMessageContent(
                            latitude: 40.7058316f,
                            longitude: -74.2581888f)    // message if result is selected
                    },

                new InlineQueryResultLocation(
                    id: "2",
                    latitude: 13.1449577f,
                    longitude: 52.507629f,
                    title: "Berlin") // displayed result
                    {

                        InputMessageContent = new InputLocationMessageContent(
                            latitude: 13.1449577f,
                            longitude: 52.507629f)   // message if result is selected
                    }
            };

            await Bot.AnswerInlineQueryAsync(
                inlineQueryEventArgs.InlineQuery.Id,
                results,
                isPersonal: true,
                cacheTime: 0);
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }
    }
}
