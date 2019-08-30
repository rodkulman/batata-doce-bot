using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Rodkulman.Telegram
{
    class Dota2
    {
        public static async Task<bool> IsDota2Reponse(Message message)
        {
            var reponses = await LoadReponses();

            return reponses[message.Text] != null;
        }

        public static async Task SendDotaResponse(Message message, string audioName)
        {
            var reponses = await LoadReponses();
            var token = reponses.Root[audioName];

            if (token != null)
            {
                var link = token.Value<string>();

                var request = WebRequest.CreateHttp(link);

                using (var response = await request.GetResponseAsync())
                {
                    await Program.Bot.SendAudioAsync(message.Chat.Id, response.GetResponseStream(), title: audioName, replyToMessageId: message.Chat.Type == ChatType.Group ? message.MessageId : 0);
                }
            }
        }

        private static async Task<JObject> LoadReponses()
        {
            using (var stream = System.IO.File.OpenRead("db/dota2-responses.json"))
            using (var textReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                return await JObject.LoadAsync(jsonReader);
            }
        }
    }
}