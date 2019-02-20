using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json.Linq;

namespace Rodkulman.Telegram
{
    public static class GoogleCloudStorage
    {
        private static readonly StorageClient storage = StorageClient.Create();

        public static async Task<MemoryStream> GetFile(string file)
        {
            var retVal = new MemoryStream();

            await storage.DownloadObjectAsync(Keys.Get("bucket"), file, retVal);

            retVal.Seek(0, SeekOrigin.Begin);

            return retVal;
        }

        public static async Task SetFile(string file, string mediaType, Stream content)
        {
            await storage.UploadObjectAsync(Keys.Get("bucket"), file, mediaType, content);
        }

        public static async Task<string> ReadAllText(string file)
        {
            using (var mem = await GetFile(file))
            using (var reader = new StreamReader(mem, true))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static async Task<string[]> ReadAllLines(string file)
        {
            var retVal = new List<string>();
            string line;

            using (var mem = await GetFile(file))
            using (var reader = new StreamReader(mem, true))
            {
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    retVal.Add(line);
                }
            }

            return retVal.ToArray();
        }

        public static IEnumerable<string> GetFiles(string folder, bool recurse = false)
        {
            var op = new ListObjectsOptions();

            foreach (var item in storage.ListObjects(Keys.Get("bucket"), folder, options: op))
            {
                if (item.Name.EndsWith('/') || (!recurse && item.Name.IndexOf('/', folder.Length + 1) > -1))
                {
                    continue;
                }

                yield return item.Name;
            }
        }

        public static async Task<JArray> GetArrayFromFile(string file)
        {
            using (var mem = await GetFile(file))
            {
                return JArray.Parse(Encoding.UTF8.GetString(mem.ToArray()));
            }
        }

        public static async Task<JObject> GetObjectFromFile(string file)
        {
            using (var mem = await GetFile(file))
            {
                return JObject.Parse(Encoding.UTF8.GetString(mem.ToArray()));
            }
        }
    }
}