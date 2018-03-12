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

            config = JObject.Parse(File.ReadAllText(@"db\config.json"));
        }

        private static void CreateConfigFile()
        {
            throw new NotImplementedException();
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
            get { return (DayOfWeek)Enum.Parse(typeof(DayOfWeek), config[nameof(GoodMorningMessageLastSent)].Value<string>()); }
            set
            {
                config[nameof(GoodMorningMessageLastSent)] = value.ToString();
                SaveConfig();
            }
        }

        public static bool ThursdayMessageSent
        {
            get { return config[nameof(ThursdayMessageSent)].Value<bool>(); }
            set
            {
                config[nameof(ThursdayMessageSent)] = value;
                SaveConfig();
            }
        }
    }
}