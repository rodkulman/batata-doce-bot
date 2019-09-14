using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;

namespace Rodkulman.Telegram
{
    public class Program
    {
        private static TelegramBotClient bot;
        private static User me;

        private static Dota2Processor dota2;
        private static WhatIsProcessor whatIs;
        private static RedditProcessor reddit;
        private static DailyMessageProcessor daily;

        public static TelegramBotClient Bot { get { return bot; } }

        public static void Main(string[] args)
        {
            Resources.Load();
            DB.Load();

            dota2 = new Dota2Processor();
            whatIs = new WhatIsProcessor();
            reddit = new RedditProcessor();
            daily = new DailyMessageProcessor();

            bot = new TelegramBotClient(DB.GetKey("Telegram"));

            bot.OnMessage += BotOnMessageReceived;
            bot.OnReceiveError += BotOnReceiveError;

            me = bot.GetMeAsync().Result;            

            bot.StartReceiving();

            Console.WriteLine($"Start listening for @{me.Username}");

            Console.ReadLine();

            bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null) { return; }

            if (message.Type != MessageType.Text) { return; }

            try
            {
                await ProcessMessage(message);
            }
            catch (Exception ex)
            {
                await bot.SendTextMessageAsync(message.Chat.Id, ex.Message);
            }
        }

        private static async Task ProcessMessage(Message message)
        {
            if (reddit.ContainsSubredditMention(message.Text))
            {
                await reddit.SendSneakPeek(message);
            }

            if (!message.Text.StartsWith("/"))
            {
                await ReplyRandomMessage(message);
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
                    await whatIs.ReplyMessage(message);
                    break;
                case "/start":
                    DB.AddChat(message.Chat.Id);
                    await bot.SendTextMessageAsync(message.Chat.Id, "Que começe a zueira");
                    break;
                case "/stop":
                    DB.RemoveChat(message.Chat.Id);
                    await bot.SendTextMessageAsync(message.Chat.Id, "toma no cu vocês");
                    break;
                case "/roll":
                    await DiceRolls.SendRollDiceMessage(message);
                    break;
                case "/dankmeme":
                    await reddit.SendRandomImage(message);
                    break;
                case "/images":
                    await SendImageList(message);
                    break;
                default:
                    if (message.Chat.Type == ChatType.Private || wasMentioned)
                    {
                        await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (message.From.Username == "rodkulman")
                        {
                            await bot.SendTextMessageAsync(message.Chat.Id, Resources.GetString("NoCommandLord"));
                        }
                        else
                        {
                            await bot.SendTextMessageAsync(message.Chat.Id, $"I'm sorry {message.From.FirstName}, I'm afraid I can't do that.");
                        }
                    }

                    break;
            }
        }

        private static async Task SendImageList(Message message)
        {
            var files = Resources.GetImages().Select(x => System.IO.Path.GetFileName(x)).OrderBy(x => x);

            var reply = "Essas são as imagens disponíveis para summonar\n\n";

            foreach (var fileName in files)
            {
                reply += fileName + "\n";
            }

            await bot.SendTextMessageAsync(message.Chat.Id, reply);
        }

        private static async Task SendTopMessage(Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            await bot.SendTextMessageAsync(message.Chat.Id, Resources.GetString("TopLink"), replyToMessageId: message.MessageId);
        }

        private static async Task SendYoAngelo(Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            await bot.SendTextMessageAsync(message.Chat.Id, Resources.GetString("YoAngelo"), replyToMessageId: message.MessageId);            
        }

        private static async Task ReplyRandomMessage(Message message)
        {
            // first, we see if it a top message
            if (message.Text.Equals("top", StringComparison.OrdinalIgnoreCase) || message.Text == "🔝")
            {
                await SendTopMessage(message);
            }

            if (message.Text == "🗿" || Regex.IsMatch(message.Text, @"^Yo,? Angelo!?$", RegexOptions.IgnoreCase))
            {
                await SendYoAngelo(message);
            }

            // then, lets see if it is a dota2 response
            if (dota2.IsDota2Reponse(message))
            {
                await dota2.SendDotaResponse(message, message.Text);
                return;
            }

            if (daily.IsThursdayReference(message.Text))
            {
                await daily.SendThurdayMessage(message.Chat.Id);
                return;
            }

            // then, we check if the message is a image request
            var match = Regex.Match(message.Text, @"^\b(?:.+?)\b\.(?:jpg|jpeg|bmp|png|gif)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                // we look for the image
                var directImage = Resources.GetImages().FirstOrDefault(x => System.IO.Path.GetFileName(x).Equals(match.Value.Trim(), StringComparison.OrdinalIgnoreCase));

                // and if we find it, send and exit
                if (!string.IsNullOrWhiteSpace(directImage))
                {
                    await bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                    using (var stream = Resources.GetFile(directImage))
                    {
                        if (match.Groups[2].Value.Equals("gif", StringComparison.OrdinalIgnoreCase))
                        {
                            await bot.SendVideoAsync(message.Chat.Id, stream, replyToMessageId: message.MessageId);
                        }
                        else
                        {
                            await bot.SendPhotoAsync(message.Chat.Id, stream, replyToMessageId: message.MessageId);
                        }
                    }

                    return;
                }
            }

            await Communism.CheckAndSendCommunismMessage(message);
        }        

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }
    }
}
