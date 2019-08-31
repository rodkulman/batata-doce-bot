using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Rodkulman.Telegram
{
    public class Dota2Processor
    {
        class DialogResponse
        {
            public string Name { get; set; }
            public AudioLink[] Links { get; set; }
        }

        class AudioLink
        {
            public string Link { get; set; }
            public string Author { get; set; }
        }

        private DialogResponse[] responses;

        public Dota2Processor()
        {
            var serializer = JsonSerializer.CreateDefault();

            using (var stream = System.IO.File.OpenRead("db/dota2-responses.json"))
            using (var textReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                responses = serializer.Deserialize<DialogResponse[]>(jsonReader);
            }
        }

        /// <summary>
        /// Determines weather a message is a Dota2 Reponse
        /// </summary>
        public bool IsDota2Reponse(Message message)
        {
            return responses.Any(x => x.Name == message.Text);
        }

        /// <summary>
        /// Sends to the chat a audio file of a Dota2 Reponse, if it exists
        /// </summary>
        public async Task SendDotaResponse(Message message, string audioName)
        {
            var dialogResponse = responses.FirstOrDefault(x => x.Name == message.Text);

            if (dialogResponse != null)
            {
                await Program.Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadAudio);

                var audioLink = dialogResponse.Links.GetRandomElement();
                var request = WebRequest.CreateHttp(audioLink.Link);

                using (var response = await request.GetResponseAsync())
                {
                    await Program.Bot.SendAudioAsync(message.Chat.Id, response.GetResponseStream(), title: audioName, performer: audioLink.Author, replyToMessageId: message.Chat.Type == ChatType.Group ? message.MessageId : 0);
                }
            }
        }
    }
}