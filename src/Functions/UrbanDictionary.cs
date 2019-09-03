using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Text.RegularExpressions;

namespace Rodkulman.Telegram
{
    public static class UrbanDictionary
    {
        private static readonly RestClient client = new RestClient("http://api.urbandictionary.com");
        private static readonly Uri urbanDictionaryLink = new Uri("https://www.urbandictionary.com");

        /// <summary>
        /// Searches for a random definition of term
        /// </summary>
        /// <param name="term">Term to seach for</param>
        /// <returns>Returns a random definition of term</returns>
        public static async Task<string> GetTermDefinition(string term)
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
                return Resources.GetString("UrbanDictionaryOff");
            }

            if (result["list"].Any())
            {
                var i = new Random().Next(result["list"].Count());

                var definition = result["list"][i];
                var permalink = definition.Value<string>("permalink");
                var definitionText = definition.Value<string>("definition");

                return $"<a href=\"{permalink}\">#{i + 1}</a>: " + Regex.Replace(definitionText, @"\[.+?\]", LinkAggregator);
            }
            else
            {
                return $"{term} not found, sorry, not sorry";
            }
        }

        private static string LinkAggregator(Match match)
        {
            var text = match.Value.Substring(1, match.Value.Length - 2);
            
            if (Uri.TryCreate(urbanDictionaryLink, $"define.php?term={text}", out Uri link)) 
            {
                return $"<a href=\"{link}\">{text}</a>";
            }
            else
            {
                return text;
            }
        }
    }
}