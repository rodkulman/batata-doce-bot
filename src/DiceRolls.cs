using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Rodkulman.Telegram
{
    public static class DiceRolls
    {
        private static readonly RestClient client = new RestClient("https://www.random.org");
        public static async Task SendRollDiceMessage(Message message)
        {
            var match = Regex.Match(message.Text, @"(?<DiceCount>\d+)?d(?<DiceSides>\d+)(?<Modifier>(\+|\-)\d+)?", RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                await Program.Bot.SendTextMessageAsync(message.Chat.Id, "Rolagens devem estar no formato XdY(+|-)Z");
                return;
            }

            var diceCount = string.IsNullOrWhiteSpace(match.Groups["DiceCount"].Value) ? 1 : int.Parse(match.Groups["DiceCount"].Value);
            var diceSides = int.Parse(match.Groups["DiceSides"].Value);
            var modifier = match.Groups["Modifier"].Value;

            var rolls = (await RequestRandomOrgNumbers(diceCount, diceSides)).ToList();

            var reply = "(";

            foreach (var roll in rolls)
            {
                if (roll == 1 || roll == diceSides)
                {
                    reply += $"*{roll}*, ";
                }
                else
                {
                    reply += $"{roll}, ";
                }
            }

            reply = reply.Substring(0, reply.Length - 2) + ") = ";

            var total = rolls.Sum();
            if (!string.IsNullOrWhiteSpace(modifier))
            {
                switch (modifier.Substring(0, 1))
                {
                    case "+":
                        total += int.Parse(modifier.Substring(1));
                        break;
                    case "-":
                        total -= int.Parse(modifier.Substring(1));
                        break;
                    default:
                        break;
                }
            }

            reply += $"*{total}*";

            await Program.Bot.SendTextMessageAsync(message.Chat.Id, reply, ParseMode.Markdown);
        }

        private static IEnumerable<int> RollDice(int diceCount, int diceSides)
        {
            var rnd = new Random();

            for (int i = 0; i < diceCount; i++)
            {
                yield return rnd.Next(0, diceSides) + 1;
            }
        }

        private static async Task<IEnumerable<int>> RequestRandomOrgNumbers(int diceCount, int diceSides)
        {
            var request = new RestRequest("integers", Method.GET);
            
            request.AddParameter("num", diceCount);
            request.AddParameter("min", 1);
            request.AddParameter("max", diceSides);
            request.AddParameter("col", 1);
            request.AddParameter("base", 10);
            request.AddParameter("format", "plain");
            request.AddParameter("rnd", true);

            var response = await client.ExecuteTaskAsync(request);

            if (response.IsSuccessful)
            {
                return response.Content.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x));
            }
            else
            {
                return RollDice(diceCount, diceSides);
            }
        }
    }
}