using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using System.Net;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types.Enums;
using System.Linq;
using RestSharp;

namespace Rodkulman.Telegram
{
    public class RedditProcessor
    {
        private static readonly RestClient client = new RestClient("https://www.reddit.com");
        private const int AllTimePostsCount = 3;

        /// <summary>
        /// Cheks weather a message contains a valid subreddit name
        /// </summary>
        public bool ContainsSubredditMention(string text)
        {
            return Regex.IsMatch(text, @"\br/.+?\b", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Sends a message with the top 3 all time posts
        /// </summary>
        public async Task SendSneakPeek(Message message)
        {
            // looks for subreddit names after ".com", as in ".com/r/all"
            var matches = Regex.Matches(message.Text, @"(?<!com/)\br/(?<Subreddit>.+?)\b", RegexOptions.IgnoreCase);

            if (matches.Any()) 
            {
                await Program.Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                foreach (Match match in matches)
                {
                    var subreddit = match.Groups["Subreddit"].Value;
                    var j = await GetSubReddit(subreddit, "top", "all", AllTimePostsCount);

                    if (j == null) { continue; }                    

                    if (j["data"].Value<int>("dist") == 0) { continue; }

                    var reply = $"<a href=\"https://www.reddit.com/r/{subreddit}/top?t=all\">Top {AllTimePostsCount} posts</a> do subreddit <a href=\"https://www.reddit.com/r/{subreddit}\">/r/{subreddit}</a>\n\n";

                    var i = 0;

                    foreach (var post in j["data"]["children"])
                    {
                        var title = post["data"].Value<string>("title");
                        var link = "https://www.reddit.com" + post["data"].Value<string>("permalink");

                        reply += $"#{++i}: <a href=\"{link}\">{title}</a>\n";
                    }

                    await Program.Bot.SendTextMessageAsync(message.Chat.Id, reply, parseMode: ParseMode.Html, replyToMessageId: message.MessageId, disableWebPagePreview: true);
                }
            }
        }

        /// <summary>
        /// Gets a subreddit posts
        /// </summary>
        private async Task<JObject> GetSubReddit(string redditName, string sort, string t = null, int limit = 0)
        {
            var request = new RestRequest($"/r/{redditName}/{sort}/.json");
            request.AddParameter("sort", sort);

            if (t != null && (sort == "controversial" || sort == "top"))
            {
                request.AddParameter("t", t);
            }

            if (limit > 0) 
            {
                request.AddParameter("limit", limit);
            }

            var response = await client.ExecuteTaskAsync(request);

            return response.IsSuccessful ? JObject.Parse(response.Content) : null;            
        }

        /// <summary>
        /// Sends a random image, from a random subreddit
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendRandomImage(Message message)
        {
            // gets the Front Page of a random subreddit
            var subredditName = System.IO.File.ReadAllLines("text-lists/meme-subreddit.txt").GetRandomElement();
            var frontPage = await GetSubReddit(subredditName, "hot");

            if (frontPage["data"].Value<int>("dist") == 0) { return; }

            await Program.Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            var imagePosts = frontPage["data"]["children"].Where(x => x["data"].Value<string>("post_hint") == "image" && isDirectImageLink(x["data"].Value<string>("url")));

            var post = imagePosts.GetRandomElement();
            
            var request = WebRequest.CreateHttp(post["data"].Value<string>("url"));
            using (var response = await request.GetResponseAsync())
            {
                await Program.Bot.SendPhotoAsync(message.Chat.Id, response.GetResponseStream(), post["data"].Value<string>("title"));
            }

            bool isDirectImageLink(string link)
            {
                return
                    link.EndsWith("png", StringComparison.OrdinalIgnoreCase) ||
                    link.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                    link.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}