using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;

namespace Rodkulman.Telegram
{
    public static class DetectLanguage
    {
        public static async Task<string> DetectFromText(string text)
        {
            var client = new RestClient("https://ws.detectlanguage.com") {
                Authenticator = new HttpBasicAuthenticator(DB.GetKey("DetectLanguage"), string.Empty)
            };

            var request = new RestRequest("/0.2/detect", Method.POST);
            request.AddParameter("q", text);

            var reponse = await client.ExecuteTaskAsync(request);

            if (reponse.IsSuccessful)
            {
                return JObject.Parse(reponse.Content)["data"]["detections"].First["language"].Value<string>();
            }
            else
            {
                return "en";
            }
        }
    }
}