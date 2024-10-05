using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Transactions;
using System;
using Tomlyn.Model;

namespace LightLoadBalancer.Controllers;

[ApiController]
[Route("/")]
public class ApiController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static int[] connectionCounts = { 0,0,0,0 };
    private static TomlArray serverIps = (TomlArray)((TomlTable)TomlConfiguration.Model["servers"]!)["ips"];
    private static TomlArray serverPorts = (TomlArray)((TomlTable)TomlConfiguration.Model["servers"]!)["ports"];
    
    public ApiController(IHttpClientFactory httpClientFactory) =>
        _httpClientFactory = httpClientFactory;
    
    //Get /
    [HttpGet]
    public async Task<ActionResult<string>> GetAsync()
    {
        var chosenServer = 0;

        for (var i = 1; i < connectionCounts.Length; i++)
        {
            if (connectionCounts[i] < connectionCounts[chosenServer])
            {
                chosenServer = i;
            }
        }
        
        try
        {
            connectionCounts[chosenServer]++;
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"http://{serverIps[chosenServer]}:{serverPorts[chosenServer]}/");

            var httpClient = _httpClientFactory.CreateClient();
            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                connectionCounts[chosenServer]--;
                return httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
            else
            {
                connectionCounts[chosenServer]--;
                var errorResponse = $"Request failed with status code: {httpResponseMessage.StatusCode}";
                return StatusCode((int)httpResponseMessage.StatusCode, errorResponse);
            }
        }
        catch (HttpRequestException exception)
        {
            connectionCounts[chosenServer]--;
            return StatusCode((int)HttpStatusCode.InternalServerError, exception.Message);
        }
    }
}