using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Rodkulman.Telegram
{
    public static class WhatIsCommand
    {
        private class FromChatUser
        {
            long chatId;
            int userId;
            public FromChatUser(long chatId, int userId)
            {
                this.chatId = chatId;
                this.userId = userId;
            }

            public long ChatId => chatId;
            public int UserId => userId;

        }
        private static List<FromChatUser> expected = new List<FromChatUser>();

        public static bool IsExpected(Message message)
        {
            return expected.Any(x => x.ChatId == message.Chat.Id && x.UserId == message.From.Id);
        }
        public static async Task ReplyMessage(Message message)
        {
            await Program.Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var tokens = message.Text.Split(' ');

            if (tokens.Length == 1 && tokens[0].StartsWith("/"))
            {
                ReplyKeyboardMarkup keyboard = new[]
                {
                    new[] { "love", "life"},
                    new[] { "man", "ti" },
                };

                expected.Add(new FromChatUser(message.Chat.Id, message.From.Id));

                await Program.Bot.SendTextMessageAsync(message.Chat.Id, "Escolhe ai", replyToMessageId: message.MessageId, replyMarkup: keyboard);
            }
            else if (tokens.Length == 1 && !tokens[0].StartsWith("/"))
            {
                var toRemove = expected.FirstOrDefault(x => x.ChatId == message.Chat.Id && x.UserId == message.From.Id);
                if (toRemove != null)
                {
                    expected.Remove(toRemove);
                    await ReplyToken(message, tokens[0]);
                }
            }
            else if (tokens.Length == 2)
            {
                await ReplyToken(message, tokens[1]);
            }
            else
            {
                await SendTermDefinition(message, String.Join("+", tokens.Skip(1)));
            }
        }

        private static async Task SendTermDefinition(Message message, string token)
        {
            string reply;

            switch (await DetectLanguage.Detect(token))
            {
                case "pt":
                    reply = DicionarioInformal.GetDefinition(token);
                    break;
                default:
                    reply = await UrbanDictionary.SendTermDefinition(token);
                    break;
            }

            await Program.Bot.SendTextMessageAsync(message.Chat.Id, reply, replyToMessageId: message.MessageId, replyMarkup: new ReplyKeyboardRemove(), parseMode: ParseMode.Html, disableWebPagePreview: true);
        }

        private static async Task ReplyToken(Message message, string token)
        {
            switch (token.ToLower())
            {
                case "love":
                    await SendReply(message, "baby don't hurt me");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await Program.Bot.SendTextMessageAsync(message.Chat.Id, "don't hurt me");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await Program.Bot.SendTextMessageAsync(message.Chat.Id, "no more");
                    break;
                case "life":
                    await SendReply(message, "42");
                    break;
                case "man":
                    await SendReply(message, "a miserable pile of secrets");
                    break;
                case "it":
                case "ti":
                    await SendReply(message, System.IO.File.ReadAllLines(@"text-replies\whatis-it.txt").GetRandomElement());
                    break;
                default:
                    await SendTermDefinition(message, token);
                    break;
            }
        }

        private static async Task SendReply(Message message, string reply)
        {
            await Program.Bot.SendTextMessageAsync(message.Chat.Id, reply, replyToMessageId: message.MessageId, replyMarkup: new ReplyKeyboardRemove());
        }
    }
}