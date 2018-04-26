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
using RestSharp;

namespace Rodkulman.Telegram
{
    public static class UrbanDictionary
    {
        private static readonly RestClient client = new RestClient("http://api.urbandictionary.com");
        public static async Task<string> SendTermDefinition(string term)
        {
            JObject result;

            var request = new RestRequest("v0/define", Method.GET);
            request.AddParameter("term", term);

            var reponse = await client.ExecuteTaskAsync(request);

            if (reponse.IsSuccessful)
            {
                result = JObject.Parse(reponse.Content);
            }
            else
            {
                return "Nem eu sei 😂";
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