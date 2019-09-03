using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Rodkulman.Telegram
{
    public class WeatherManager
    {
        private readonly RestClient client = new RestClient("http://api.openweathermap.org");
        
        public const int PortoAlegreId = 3452925;
        public const int TresDeMaioId = 3446130;

        public async Task<JObject> GetWeatherForCity(int cityId)
        {
            var request = new RestRequest($"data/2.5/weather", Method.GET);
            request.AddParameter("id", cityId);
            request.AddParameter("APPID", DB.GetKey("OpenWeatherMap"));
            request.AddParameter("units", "metric");

            var response = await client.ExecuteTaskAsync(request);

            if (response.IsSuccessful)
            {
                return JObject.Parse(response.Content);
            }
            else
            {
                return null;
            }
        }

        public async Task<double> GetMaxTempForCity(int cityId)
        {
            var weather = await GetWeatherForCity(cityId);

            if (weather == null || (weather.TryGetValue("cod", out JToken cod) && cod.Value<int>() == 429))
            {
                return 0.0;
            }

            return weather.GetValue("main").Value<double>("temp_max");
        }
    }
}