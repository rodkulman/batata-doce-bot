using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace Rodkulman.Telegram
{
    // https://stackoverflow.com/questions/27846337/select-and-download-random-image-from-google/27847293#27847293
    public static class GoogleImages
    {
        public static async Task<Stream> GetRandomImage(string topic)
        {
            var uri = (await GoogleImages.GetImages(topic)).GetRandomElement();

            var request = WebRequest.CreateHttp(uri);

            using (var response = await request.GetResponseAsync())
            {
                return response.GetResponseStream();
            }
        }

        public static async Task<Uri[]> GetImages(string topic)
        {
            string data;

            var request = WebRequest.CreateHttp($"https://www.google.com/search?q={topic}&tbm=isch");
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";

            using (var response = await request.GetResponseAsync())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                data = await reader.ReadToEndAsync();
            }

            return ParseUrls(data).ToArray();
        }

        private static IEnumerable<Uri> ParseUrls(string html)
        {
            var ndx = html.IndexOf("\"ou\"", StringComparison.Ordinal);

            while (ndx >= 0)
            {
                ndx = html.IndexOf("\"", ndx + 4, StringComparison.Ordinal) + 1;

                var ndx2 = html.IndexOf("\"", ndx, StringComparison.Ordinal);
                var url = new Uri(html.Substring(ndx, ndx2 - ndx));

                yield return url;

                ndx = html.IndexOf("\"ou\"", ndx2, StringComparison.Ordinal);
            }
        }
    }
}