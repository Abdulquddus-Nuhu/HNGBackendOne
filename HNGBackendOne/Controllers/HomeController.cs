using HNGBackendOne.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HNGBackendOne.Controllers
{
    [Route("api/")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string visitor_name)
        {
            try
            {
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                var location = await GetLocationFromIp(clientIp);
                if (location == "Localhost")
                {
                    return BadRequest(new { message = "You Ip address is showing localhost" });
                }

                var temperature = await GetTemperatureForLocation(location);
                var greeting = $"Hello, {visitor_name}!, the temperature is {temperature} degrees Celsius in {location}";

                var response = new
                {
                    client_ip = clientIp,
                    location = location,
                    greeting = greeting
                };

                return Ok(response);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Internal server error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task<string> GetLocationFromIp(string ip)
        {
            _logger.LogInformation($"Ip: {ip}");

            if (string.IsNullOrEmpty(ip) || ip == "::1" || ip == "127.0.0.1")
            {
                return "Localhost";
            }

            var apiKey = Environment.GetEnvironmentVariable("IPGEOLOCATION_API_KEY");
            var apiUrl = $"https://api.ipgeolocation.io/ipgeo?apiKey={apiKey}&ip={ip}";

            var client = _httpClientFactory.CreateClient();
            var response = await  client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error fetching location. Status Code: {response.StatusCode}, Response: {content}");
                throw new HttpRequestException($"Error fetching location. Status Code: {response.StatusCode}, Response: {content}");
            }

            var locationResponse = await response.Content.ReadFromJsonAsync<IpGeolocationResponse>();
            var city = locationResponse.city;

            _logger.LogInformation($"City: {city}");

            return locationResponse.city;
        }

        private async Task<double> GetTemperatureForLocation(string location)
        {
            var apiKey = Environment.GetEnvironmentVariable("WEATHER_API_KEY");
            var apiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units=metric&appid={apiKey}";

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error fetching temperature. Status Code: {response.StatusCode}, Response: {content}");
                throw new HttpRequestException($"Error fetching temperature. Status Code: {response.StatusCode}, Response: {content}");
            }

            var weatherResponse = await response.Content.ReadFromJsonAsync<WeatherApiResponse>();
            return weatherResponse.Main.Temp;
        }
    }
}
