using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LightLoadBalancer.Controllers;

[ApiController]
[Route("/")]
public class ApiController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    public ApiController(IHttpClientFactory httpClientFactory) =>
        _httpClientFactory = httpClientFactory;
    
    //Get /
    [HttpGet]
    public async Task<ActionResult<string>> GetAsync()
    {
        try
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost:80/");

            var httpClient = _httpClientFactory.CreateClient();
            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
            else
            {
                var errorResponse = $"Request failed with status code: {httpResponseMessage.StatusCode}";
                return StatusCode((int)httpResponseMessage.StatusCode, errorResponse);
            }
        }
        catch (HttpRequestException exception)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, exception.Message);
        }
    }
}