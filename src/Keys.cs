using System.IO;
using Newtonsoft.Json.Linq;

namespace Rodkulman.Telegram
{
    public static class Keys
    {
        public static string Get(string keyName)
        {
            var keys = JObject.Parse(File.ReadAllText(@"db\keys.json"));

            return keys[keyName].Value<string>();
        }
    }
}