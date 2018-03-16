using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rodkulman.Telegram
{
    public static class DB
    {
        private static readonly JObject config = LoadOrCreateConfig();

        private static readonly object lockKey = new object();

        private static JObject LoadOrCreateConfig()
        {
            if (!File.Exists(@"db\config.json"))
            {
                return new JObject(
                    new JProperty(nameof(GoodMorningMessageLastSent), DateTime.Now.DayOfWeek),
                    new JProperty(nameof(ThursdayMessageSent), false),
                    new JProperty(nameof(WednesdayMyDudes), false)
                );
            }
            else
            {
                return JObject.Parse(File.ReadAllText(@"db\config.json"));
            }
        }

        private static void SaveConfig()
        {
            lock (lockKey)
            {
                using (var stream = File.OpenWrite(@"db\config.json"))
                using (var textWriter = new StreamWriter(stream))
                using (var jsonWriter = new JsonTextWriter(textWriter))
                {
                    config.WriteTo(jsonWriter);
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
    }
}