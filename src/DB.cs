using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rodkulman.Telegram
{
    public static class DB
    {
        private static readonly JObject config = GoogleCloudStorage.GetObjectFromFile("db/config.json").Result;

        private static readonly object lockKey = new object();

        private static void SaveConfig()
        {
            lock (lockKey)
            {
                using (var mem = new MemoryStream())
                {
                    using (var textWriter = new StreamWriter(mem, Encoding.UTF8, 1024, true))
                    using (var jsonWriter = new JsonTextWriter(textWriter))
                    {
                        config.WriteTo(jsonWriter);
                    }

                    mem.Seek(0, SeekOrigin.Begin);
                    GoogleCloudStorage.SetFile("db/config.json", "text/plain", mem).Wait();
                }
            }
        }

        public static DayOfWeek GoodMorningMessageLastSent
        {
            get { return (DayOfWeek)config.Value<int>(nameof(GoodMorningMessageLastSent)); }
            set
            {
                config[nameof(GoodMorningMessageLastSent)] = (int)value;
                SaveConfig();
            }
        }

        public static bool WednesdayMyDudes
        {
            get { return config.Value<bool>(nameof(WednesdayMyDudes)); }
            set
            {
                config[nameof(WednesdayMyDudes)] = value;
                SaveConfig();
            }
        }

        public static bool ThursdayMessageSent
        {
            get { return config.Value<bool>(nameof(ThursdayMessageSent)); }
            set
            {
                config[nameof(ThursdayMessageSent)] = value;
                SaveConfig();
            }
        }

        public static bool GottaGetDownOnFriday
        {
            get { return config.Value<bool>(nameof(GottaGetDownOnFriday)); }
            set
            {
                config[nameof(GottaGetDownOnFriday)] = value;
                SaveConfig();
            }
        }
    }
}