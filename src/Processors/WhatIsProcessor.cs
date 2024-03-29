using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Rodkulman.Telegram
{
    public class WhatIsProcessor
    {
        public async Task ReplyMessage(Message message)
        {
            await Program.Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var tokens = message.Text.Split(' ');

            if (tokens.Count() == 1)
            {
                await SendReply(message, "Formato do comando é /whatis palavra", message.Chat.Type == ChatType.Group);
            }
            else
            {
                await SendTermDefinition(message, String.Join("+", tokens.Skip(1)));
            }
        }

        private async Task SendTermDefinition(Message message, string token)
        {
            string reply;

            switch (await DetectLanguage.DetectFromText(token))
            {
                case "pt":
                    reply = DicionarioInformal.GetDefinition(token);
                    break;
                default:
                    reply = await UrbanDictionary.GetTermDefinition(token);
                    break;
            }

            await Program.Bot.SendTextMessageAsync(message.Chat.Id, reply, parseMode: ParseMode.Html, disableWebPagePreview: true);
        }

        private async Task SendReply(Message message, string reply, bool shouldReplyTo)
        {
            await Program.Bot.SendTextMessageAsync(message.Chat.Id, reply, replyToMessageId: shouldReplyTo ? message.MessageId : 0, replyMarkup: new ReplyKeyboardRemove());
        }
    }
}