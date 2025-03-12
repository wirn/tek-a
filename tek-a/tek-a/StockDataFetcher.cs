using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public static class StockDataFetcher
{
    private static readonly HttpClient _httpClient = new HttpClient();

    [FunctionName("FetchPolygonData")]
    public static async Task<HttpResponseMessage> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestMessage req,
        ILogger log)
    {
        log.LogInformation("Fetching data from Polygon.io...");

        try
        {
            string apiKey = Environment.GetEnvironmentVariable("PolygonApiKey");
            string url = $"https://api.polygon.io/v2/aggs/ticker/ASML/range/1/day/2024-12-01/2025-03-10?adjusted=true&sort=asc&apiKey={apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<object>(responseBody);

            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(jsonResponse), System.Text.Encoding.UTF8, "application/json")
            };
        }
        catch (Exception ex)
        {
            log.LogError($"Error fetching data: {ex.Message}");
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent($"Error: {ex.Message}")
            };
        }
    }
}
