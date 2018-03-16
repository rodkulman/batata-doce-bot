using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rodkulman.Telegram
{
    public static class DB
    {
        static DB()
        {
            if (!File.Exists(@"db\config.json"))
            {
                CreateConfigFile();
            }
            else
            {
                config = JObject.Parse(File.ReadAllText(@"db\config.json"));
            }
        }

        private static void CreateConfigFile()
        {
            config = new JObject(
                new JProperty(nameof(GoodMorningMessageLastSent), DateTime.Now.DayOfWeek),
                new JProperty(nameof(ThursdayMessageSent), false),
                new JProperty(nameof(WednesdayMyDudes), false)
            );

            SaveConfig();
        }

        private static void SaveConfig()
        {
            using (var stream = File.OpenWrite(@"db\config.json"))
            using (var textWriter = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                config.WriteTo(jsonWriter);
            }
        }

        private static JObject config;

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