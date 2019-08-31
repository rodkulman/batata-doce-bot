using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Rodkulman.Telegram
{
    public class Dota2Processor
    {
        private XDocument allResponses;

        public Dota2Processor()
        {
            using (var stream = Resources.GetFile("resources/dota2-dialogs.xml"))
            {
                allResponses = XDocument.Load(stream);
            }            
        }

        /// <summary>
        /// Determines weather a message is a Dota2 Reponse
        /// </summary>
        public bool IsDota2Reponse(Message message)
        {
            return allResponses.Root.Elements("Dialog").Any(x => x.Attribute("Name").Value == message.Text);
        }

        /// <summary>
        /// Sends to the chat a audio file of a Dota2 Reponse, if it exists
        /// </summary>
        public async Task SendDotaResponse(Message message, string audioName)
        {
            var dialogResponse = allResponses.Root.Elements("Dialog").FirstOrDefault(x => x.Attribute("Name").Value == audioName);

            if (dialogResponse != null)
            {
                await Program.Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadAudio);

                var audioLink = dialogResponse.Elements("Link").GetRandomElement();
                var request = WebRequest.CreateHttp(audioLink.Value);

                using (var response = await request.GetResponseAsync())
                {
                    await Program.Bot.SendAudioAsync(message.Chat.Id, response.GetResponseStream(), title: audioName, performer: audioLink.Attribute("Author").Value, replyToMessageId: message.Chat.Type == ChatType.Group ? message.MessageId : 0);
                }
            }
        }
    }
}