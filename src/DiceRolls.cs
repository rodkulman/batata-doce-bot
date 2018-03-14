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
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Rodkulman.Telegram
{
    public static class DiceRolls
    {
        private static string key;
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

            // var rolls = await RequestRandomOrgNumbers(diceCount, diceSides);
            var rolls = RollDice(diceCount, diceSides).ToList();

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
            if (string.IsNullOrWhiteSpace(key))
            {
                key = JObject.Parse(System.IO.File.ReadAllText("keys.json")).Value<string>("Random.org");
            }

            var request = WebRequest.CreateHttp("https://api.random.org/json-rpc/1/invoke");
            request.ContentType = "application/json-rpc";
            request.Method = "POST";

            var jRequest = new JObject(
                new JProperty("jsonrpc", "2.0"),
                new JProperty("method", "generateIntegers"),
                new JProperty("params", new JObject(
                    new JProperty("apiKey", key),
                    new JProperty("n", diceCount),
                    new JProperty("min", 1),
                    new JProperty("max", diceSides),
                    new JProperty("replacement", true)
                )),
                new JProperty("id", 42)
            );

            using (var writer = new StreamWriter(request.GetRequestStream(), Encoding.UTF8, 1024, true))
            using (var jsonWriter = new JsonTextWriter(writer) { Formatting = Formatting.None })
            {
                jRequest.WriteTo(jsonWriter);
            }

            JObject jResponse;

            using (var response = await request.GetResponseAsync())
            using (var reader = new StreamReader(response.GetResponseStream()))
            using (var jsonReader = new JsonTextReader(reader))
            {
                jResponse = JObject.Load(jsonReader);
            }

            return jResponse["result"]["random"].Values<int>("data");
        }
    }
}