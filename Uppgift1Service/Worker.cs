using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Uppgift1Service.Models;

namespace Uppgift1Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string _url = "https://api.openweathermap.org/data/2.5/onecall?lat=59.2741668&lon=15.2139959&exclude=hourly,daily&units=metric&appid=7fd7db6d0ff6e8c22433efb93de3f6b6";
        private HttpClient _client;
        private HttpResponseMessage _response;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        //En Start funktion av typen async som innehåller en ny instance av httpclient och loggar ut meddelandet
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            
            _client = new HttpClient();
            _logger.LogInformation("The service has been started.");
            return base.StartAsync(cancellationToken);
        }

        //stop funktionen (async) som tar bort data från ram minnet och loggar ut meddelandet
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            //städar upp efter oss - tar bort från RAM
            _client.Dispose();
            _logger.LogInformation("The service has been stopped.");
            return base.StopAsync(cancellationToken);
        }

        //Uför servicen
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Så länge cancel request inte är true
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //hämtar url i en annan tråd
                    _response = await _client.GetAsync(_url);
                   //om status koden är 2xx
                    if (_response.IsSuccessStatusCode)
                    {
                        
                        //json meddelanden
                        var result = await _response.Content.ReadAsStringAsync();
                       //häntar värdet från json och parsa som sedan omvandlar till en objekt med datatypen double
                        var temperature = JObject.Parse(result)["current"]["temp"].ToObject<double>();
                        var humidity = JObject.Parse(result)["current"]["humidity"].ToObject<double>();
                       
                        var timezone = JObject.Parse(result)["timezone"];
                 
                        _logger.LogInformation(temperature > 21 ? $"Temperaturen: {temperature}C överstiger 20C" : $"Temperaturen: {temperature}C understiger 20C");
                        _logger.LogInformation($"Fuktigheten i {timezone} är {humidity}\n");
                    }
                    else 
                    {
                        _logger.LogInformation($"The website ({_url}) is down. Status Code = {_response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Failed. The website ({_url}) - {ex.Message}");
                }

                await Task.Delay(60 * 1000, stoppingToken);
            }
        }
    }
}
