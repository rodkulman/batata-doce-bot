using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Rodkulman.Telegram
{
    public static class UrbanDictionary
    {
        private static readonly RestClient client = new RestClient("http://api.urbandictionary.com");

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
                var definition = result["list"].GetRandomElement();

                return $"<a href=\"{definition.Value<string>("permalink")}\">{definition.Value<string>("definition")}</a>";
            }
            else
            {
                return $"{term} not found, sorry, not sorry";
            }
        }
    }
}