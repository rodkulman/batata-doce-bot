using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Linq;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace Rodkulman.Telegram
{
    public static class UrbanDictionary
    {
        public static async Task<string> SendTermDefinition(string term)
        {
            var request = WebRequest.CreateHttp($"http://api.urbandictionary.com/v0/define?term={term}");
            JObject result;

            using (var response = await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            using (var textReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                result = await JObject.LoadAsync(jsonReader);
            }

            if (result["list"].Any())
            {
                var definition = result["list"].GetRandomElement();

                return $"<a href=\"{definition.Value<string>("permalink")}\">{definition.Value<string>("definition")}</a>";
            }
            else
            {
                return "Nem eu sei 😂";
            }
        }
    }
}