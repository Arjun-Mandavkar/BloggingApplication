using BloggingApplication.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloggingApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN,BLOGGER")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation("_________In WeatherForecastController first log");

            IEnumerable<Claim> list = HttpContext.User.Claims;
            string id = list.FirstOrDefault(c => c.Type == "Id").Value;
            string name = list.FirstOrDefault(c => c.Type == "Name").Value;
            string email = list.FirstOrDefault(c => c.Type == "Email").Value;
            string roleType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            string role = list.FirstOrDefault(c => c.Type == roleType).Value;
            Console.WriteLine($"ID: {id}\nName: {name}\nEmail: {email}\nRole: {role}");


            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}