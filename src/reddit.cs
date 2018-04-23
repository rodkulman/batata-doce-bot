using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using System.Linq;

namespace Rodkulman.Telegram
{
    public static class Reddit
    {
        public static bool ContainsSubredditMention(string text)
        {
            return Regex.IsMatch(text, @"\br/.+?\b", RegexOptions.IgnoreCase);
        }

        public async static Task SendSneakPeek(Message message)
        {
            foreach (Match match in Regex.Matches(message.Text, @"(?<!com/)\br/(?<Subreddit>.+?)\b", RegexOptions.IgnoreCase))
            {
                var j = await GetRedditTop3(match.Groups["Subreddit"].Value);
                var subreddit = match.Groups["Subreddit"].Value;

                if (j["data"].Value<int>("dist") == 0) { continue; }

                var reply = $"Aqui estão os <a href=\"https://www.reddit.com/r/{subreddit}/top?t=all\">top 3 de todos os tempos</a> do <a href=\"https://www.reddit.com/r/{subreddit}\">/r/{subreddit}</a>\n\n";

                var i = 0;

                foreach (var post in j["data"]["children"])
                {
                    var title = post["data"].Value<string>("title");
                    var link = "https://www.reddit.com" + post["data"].Value<string>("permalink");

                    reply += $"#{++i}: <a href=\"{link}\">{title}</a>\n";
                }

                await Program.Bot.SendTextMessageAsync(message.Chat.Id, reply, parseMode: ParseMode.Html, replyToMessageId: message.MessageId, disableWebPagePreview: true, replyMarkup: new ReplyKeyboardRemove());
            }
        }

        public static async Task GetRandomImage(Message message, string redditName)
        {
            var j = await GetSubReddit(redditName, "hot", null);

            if (j["data"].Value<int>("dist") == 0) { return; }

            if (j["data"]["children"].Where(x => x["data"].Value<string>("post_hint") == "image" && isDirectImageLink(x["data"].Value<string>("url"))).GetRandomElement() is JObject post)
            {
                var request = WebRequest.CreateHttp(post["data"].Value<string>("url"));

                using (var response = await request.GetResponseAsync())
                {
                    await Program.Bot.SendPhotoAsync(message.Chat.Id, response.GetResponseStream(), post["data"].Value<string>("title"));
                }
            }

            bool isDirectImageLink(string link)
            {
                return
                    link.EndsWith("png", StringComparison.OrdinalIgnoreCase) ||
                    link.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                    link.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase);
            }
        }

        private static async Task<JObject> GetRedditTop3(string redditName)
        {
            var request = WebRequest.CreateHttp($"https://www.reddit.com/r/{redditName}/top/.json?t=all&limit=3");

            using (var response = await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            using (var textReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                return await JObject.LoadAsync(jsonReader);
            }
        }

        private static async Task<JObject> GetSubReddit(string redditName, string sort, string t)
        {
            var url = $"https://www.reddit.com/r/{redditName}/{sort}.json?sort={sort}";

            if (sort == "controversial" || sort == "top")
            {
                url += $"&t={t}";
            }

            var request = WebRequest.CreateHttp(url);

            using (var response = await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            using (var textReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                return await JObject.LoadAsync(jsonReader);
            }
        }
    }
}