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

        //En Start funktion av typen async som inneh�ller en ny instance av httpclient och loggar ut meddelandet
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            
            _client = new HttpClient();
            _logger.LogInformation("The service has been started.");
            return base.StartAsync(cancellationToken);
        }

        //stop funktionen (async) som tar bort data fr�n ram minnet och loggar ut meddelandet
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            //st�dar upp efter oss - tar bort fr�n RAM
            _client.Dispose();
            _logger.LogInformation("The service has been stopped.");
            return base.StopAsync(cancellationToken);
        }

        //Uf�r servicen
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //S� l�nge cancel request inte �r true
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //h�mtar url i en annan tr�d
                    _response = await _client.GetAsync(_url);
                   //om status koden �r 2xx
                    if (_response.IsSuccessStatusCode)
                    {
                        
                        //json meddelanden
                        var result = await _response.Content.ReadAsStringAsync();
                       //h�ntar v�rdet fr�n json och parsa som sedan omvandlar till en objekt med datatypen double
                        var temperature = JObject.Parse(result)["current"]["temp"].ToObject<double>();
                        var humidity = JObject.Parse(result)["current"]["humidity"].ToObject<double>();
                       
                        var timezone = JObject.Parse(result)["timezone"];
                 
                        _logger.LogInformation(temperature > 21 ? $"Temperaturen: {temperature}C �verstiger 20C" : $"Temperaturen: {temperature}C understiger 20C");
                        _logger.LogInformation($"Fuktigheten i {timezone} �r {humidity}\n");
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
