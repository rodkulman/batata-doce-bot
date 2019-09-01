using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rodkulman.Telegram
{
    public static class Resources
    {
        private static JObject strings;

        public static void Load()
        {
            using (var stream = File.OpenRead("resources/strings.json"))
            using (var textReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                strings = JObject.Load(jsonReader);
            }
        }

        public static string GetString(string name)
        {
            return strings[name].Value<string>();
        }

        public static IEnumerable<string> GetStrings(string name)
        {
            return strings[name].Values<string>();
        }

        public static IEnumerable<string> GetImages()
        {
            return Directory.EnumerateFiles("resources/images");
        }

        public static Stream GetAudio(string name)
        {
            return GetFile("resources/audio/" + name);
        }

        public static Stream GetFile(string path)
        {
            return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}