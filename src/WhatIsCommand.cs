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
        private static List<long> expected = new List<long>();

        public static bool IsExpected(Message message)
        {
            return expected.Any(x => x == message.Chat.Id);
        }
        public static async Task ReplyMessage(Message message, CallbackQuery query = null)
        {
            await Program.Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            string[] tokens;

            if (query == null)
            {
                tokens = message.Text.Split(' ');
            }
            else
            {
                tokens = query.Data.Split(' ');
            }

            if (tokens.Length == 1 && tokens[0].StartsWith("/"))
            {
                IReplyMarkup keyboard;

                if (message.Chat.Type == ChatType.Group)
                {
                    keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("love"),
                            InlineKeyboardButton.WithCallbackData("life"),
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("man"),
                            InlineKeyboardButton.WithCallbackData("ti"),
                        }
                    });

                    await Program.Bot.SendTextMessageAsync(message.Chat.Id, "Escolhe ai", replyToMessageId: message.MessageId, replyMarkup: keyboard);
                }
                else
                {
                    keyboard = (ReplyKeyboardMarkup)new[]
                    {
                        new[] { "love", "life"},
                        new[] { "man", "ti" },
                    };

                    expected.Add(message.Chat.Id);
                    await Program.Bot.SendTextMessageAsync(message.Chat.Id, "Pergunta ai o que tu quer", replyToMessageId: message.MessageId, replyMarkup: keyboard);
                }
            }
            else if (query != null)
            {
                await ReplyToken(message, tokens[0], shouldReplyTo: false, prefix: $"{query.From.FirstName} asked what is {string.Join(' ', tokens)}");
            }
            else if (tokens.Length == 1 && !tokens[0].StartsWith("/"))
            {
                var toRemove = expected.FirstOrDefault(x => x == message.Chat.Id);
                if (toRemove != default(long))
                {
                    expected.Remove(toRemove);
                    await ReplyToken(message, tokens[0], shouldReplyTo: query == null);
                }
            }
            else if (tokens.Length == 2)
            {
                await ReplyToken(message, tokens[1], shouldReplyTo: query == null);
            }
            else
            {
                await SendTermDefinition(message, String.Join("+", tokens.Skip(1)), shouldReplyTo: query == null);
            }
        }

        private static async Task SendTermDefinition(Message message, string token, bool shouldReplyTo)
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

            await Program.Bot.SendTextMessageAsync(message.Chat.Id, reply, replyToMessageId: shouldReplyTo ? message.MessageId : 0, replyMarkup: new ReplyKeyboardRemove(), parseMode: ParseMode.Html, disableWebPagePreview: true);
        }

        private static async Task ReplyToken(Message message, string token, bool shouldReplyTo, string prefix = null)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                await SendReply(message, prefix, shouldReplyTo);
            }

            switch (token.ToLower())
            {
                case "love":
                    await SendReply(message, "baby don't hurt me", shouldReplyTo);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await Program.Bot.SendTextMessageAsync(message.Chat.Id, "don't hurt me");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await Program.Bot.SendTextMessageAsync(message.Chat.Id, "no more");
                    break;
                case "life":
                    await SendReply(message, "42", shouldReplyTo);
                    break;
                case "man":
                    await SendReply(message, "a miserable pile of secrets", shouldReplyTo);
                    break;
                case "it":
                case "ti":
                    await SendReply(message, (await GoogleCloudStorage.ReadAllLines("text-replies/whatis-it.txt")).GetRandomElement(), shouldReplyTo);
                    break;
                default:
                    await SendTermDefinition(message, token, shouldReplyTo);
                    break;
            }
        }

        private static async Task SendReply(Message message, string reply, bool shouldReplyTo)
        {
            await Program.Bot.SendTextMessageAsync(message.Chat.Id, reply, replyToMessageId: shouldReplyTo ? message.MessageId : 0, replyMarkup: new ReplyKeyboardRemove());
        }
    }
}