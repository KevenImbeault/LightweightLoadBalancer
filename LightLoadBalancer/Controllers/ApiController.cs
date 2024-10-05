using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Transactions;
using System;

namespace LightLoadBalancer.Controllers;

[ApiController]
[Route("/")]
public class ApiController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static int[] connectionCount = { 0,0,0,0 };
    
    public ApiController(IHttpClientFactory httpClientFactory) =>
        _httpClientFactory = httpClientFactory;
    
    //Get /
    [HttpGet]
    public async Task<ActionResult<string>> GetAsync()
    {
        try
        {
            connectionCount[0]++;
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost:80/");

            var httpClient = _httpClientFactory.CreateClient();
            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                connectionCount[0]--;
                return httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
            else
            {
                connectionCount[0]--;
                var errorResponse = $"Request failed with status code: {httpResponseMessage.StatusCode}";
                return StatusCode((int)httpResponseMessage.StatusCode, errorResponse);
            }
        }
        catch (HttpRequestException exception)
        {
            connectionCount[0]--;
            return StatusCode((int)HttpStatusCode.InternalServerError, exception.Message);
        }
    }
}