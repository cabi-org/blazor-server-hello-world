using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Cabi.KubernetesTechDemo.Web.Data
{
    public class WeatherForecastService
    {
        private readonly IConfiguration _configuration;

        public WeatherForecastService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
        {
            using HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Simple Blazor Server Application");

            string sandboxDomain = _configuration.GetValue<string>("CabiUrls:MySandbox");
            var streamTask = await client.GetStreamAsync($"https://{sandboxDomain}/api/weatherforecast");
            var forecast = await JsonSerializer.DeserializeAsync<List<WeatherForecast>>(streamTask);

            return forecast.ToArray();
        }
    }
}
