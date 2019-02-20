using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rodkulman.Telegram
{
    public static class Keys
    {
        private static JObject keys = null;
        public static string Get(string keyName)
        {
            if (keys == null)
            {
                using (var file = File.OpenRead(@"db\keys.json"))
                using (var reader = new StreamReader(file, Encoding.UTF8))
                using (var jReader = new JsonTextReader(reader))
                {
                    keys = JObject.Load(jReader);
                }
            }

            return keys[keyName].Value<string>();
        }
    }
}