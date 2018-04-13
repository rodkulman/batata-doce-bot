using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using RestSharp;
using RestSharp.Authenticators;

namespace Rodkulman.Telegram
{
    public static class DetectLanguage
    {
        public static async Task<string> Detect(string text)
        {
            var client = new RestClient("https://ws.detectlanguage.com") {
                Authenticator = new HttpBasicAuthenticator(Keys.Get("detectLanguage"), string.Empty)
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