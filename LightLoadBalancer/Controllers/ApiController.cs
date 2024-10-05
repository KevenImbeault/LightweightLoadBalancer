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
    private static TomlArray serverIps = (TomlArray)((TomlTable)TomlConfiguration.Model["servers"]!)["ips"];
    private static TomlArray serverPorts = (TomlArray)((TomlTable)TomlConfiguration.Model["servers"]!)["ports"];
    
    private static int[] connectionCounts = new int[serverIps.Count];
    
    public ApiController(IHttpClientFactory httpClientFactory) =>
        _httpClientFactory = httpClientFactory;

    private async Task<int> GetLeastConnectionServer()
    {
        return await Task.Run(() =>
        {
            var chosenServer = 0;

            for (var i = 1; i < connectionCounts.Length; i++)
            {
                if (connectionCounts[i] == -1)
                {
                    continue;
                }
                
                if (connectionCounts[i] < connectionCounts[chosenServer] || connectionCounts[chosenServer] == -1)
                {
                    chosenServer = i;
                }
            }
            
            return chosenServer;
        });
    }
    
    //Get /
    [HttpGet]
    public async Task<ActionResult<string>> GetAsync()
    {
        var httpClient = _httpClientFactory.CreateClient();

        var chosenServer = await GetLeastConnectionServer();
        var serverAlive = false;

        do
        {
            try
            {
                var aliveRequestMessage = new HttpRequestMessage(HttpMethod.Get,
                    $"http://{serverIps[chosenServer]}:{serverPorts[chosenServer]}/alive");
                var aliveResponseMessage = await httpClient.SendAsync(aliveRequestMessage);

                if (aliveResponseMessage.IsSuccessStatusCode)
                {
                    serverAlive = true;
                }
            }
            catch (HttpRequestException exception)
            {
                connectionCounts[chosenServer] = -1;
                chosenServer = await GetLeastConnectionServer();
            }
        } while (!serverAlive);
        
        try
        {
            connectionCounts[chosenServer]++;
            
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"http://{serverIps[chosenServer]}:{serverPorts[chosenServer]}/");
            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                connectionCounts[chosenServer]--;
                return httpResponseMessage.Content.ReadAsStringAsync().Result;
            }

            connectionCounts[chosenServer]--;
            var errorResponse = $"Request failed with status code: {httpResponseMessage.StatusCode}";
            return StatusCode((int)httpResponseMessage.StatusCode, errorResponse);
        }
        catch (HttpRequestException exception)
        {
            connectionCounts[chosenServer]--;
            return StatusCode((int)HttpStatusCode.InternalServerError, exception.Message);
        }
    }
}