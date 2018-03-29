using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Rodkulman.Telegram
{
    public static class Communism
    {
        public async static Task CorrectCapitalistInfidels(Message message)
        {
            if (Regex.Matches(message.Text, @"(meus?|minhas?) \b([a-z]+)\b|\b([a-z]+)\b é (meus?|minhas?)", RegexOptions.IgnoreCase) is MatchCollection matches && matches.Any(x => x.Success))
            {
                var reply = "Acho que tu quis dizer";

                if (matches.Count > 1)
                {
                    reply += ":\n";

                    foreach (Match match in matches)
                    {
                        reply += $"*{GetCommunistResponse(match)}*\n";
                    }

                    reply += "Não é, camarada?";
                }
                else
                {
                    reply += $" *{GetCommunistResponse(matches[0])}*, né camarada?";
                }

                await Program.Bot.SendTextMessageAsync(message.Chat.Id, reply, replyToMessageId: message.MessageId, parseMode: ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove());
            }

            string GetCommunistResponse(Match match)
            {
                if (string.IsNullOrWhiteSpace(match.Groups[1].Value))
                {
                    return GetPossessive(match.Groups[4].Value) + match.Groups[3].Value.ToLower();
                }
                else
                {
                    return GetPossessive(match.Groups[1].Value) + match.Groups[2].Value.ToLower();
                }

                string GetPossessive(string wrongPossessive)
                {
                    var plural = wrongPossessive.EndsWith("s", StringComparison.OrdinalIgnoreCase);

                    if (wrongPossessive.StartsWith("meu"))
                    {
                        return "nosso" + (plural ? "s" : string.Empty);
                    }
                    else
                    {
                        return "nossa" + (plural ? "s" : string.Empty);
                    }
                }
            }
        }
    }
}