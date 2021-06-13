using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Serialization;

namespace Cabi.KubernetesTechDemo.Web.Data
{
    public class WeatherForecast
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("temperatureC")]
        public int TemperatureC { get; set; }

        [JsonPropertyName("temperatureF")]
        public int TemperatureF { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }
    }
}
