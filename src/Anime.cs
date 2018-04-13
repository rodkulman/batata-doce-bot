using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using RestSharp;
using RestSharp.Authenticators;

namespace Rodkulman.Telegram
{
    public static class Anime
    {
        public static IEnumerable<string> GetAnimes(string message)
        {
            var matches = Regex.Matches(message, "{.+?}");

            foreach (Match match in matches)
            {
                yield return match.Value.Substring(1, match.Value.Length - 2);
            }
        }

        public static async Task<Uri> GetMALLink(string anime)
        {
            var client = new RestClient("https://myanimelist.net"){
                Authenticator = new HttpBasicAuthenticator(Keys.Get("MALUser"), Keys.Get("MALPassword")),
                UserAgent = "batata-doce-bot"
            };

            var request = new RestRequest($"/api/anime/search.xml?q={String.Join("+", anime.Split(' '))}", Method.GET);
            var response = await client.ExecuteTaskAsync(request);

            if (response.IsSuccessful)
            {
                var doc = XDocument.Parse(response.Content);
                var id = (int)doc.Root.Elements("entry").First().Element("id");

                return new Uri($"https://myanimelist.net/anime/{id}");
            }
            else
            {
                return null;
            }
        }
    }
}