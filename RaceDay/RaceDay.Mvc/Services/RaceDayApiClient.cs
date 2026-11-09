using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Mvc.Services
{
    public class RaceDayApiClient
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RaceDayApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor)
        {
            _factory = factory;
            _httpContextAccessor = accessor;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null)
        {
            var client = _factory.CreateClient("RaceDayApi");
            var request = new HttpRequestMessage(method, url);

            // 1. Retrieve the API cookie from MVC session
            string? apiCookie = _httpContextAccessor.HttpContext?.Session.GetString("ApiSessionCookie");

            // 2. Attach it to the request
            if (!string.IsNullOrWhiteSpace(apiCookie))
            {
                request.Headers.TryAddWithoutValidation("Cookie", apiCookie);
            }

            // 3. Add body if provided
            if (body != null)
            {
                request.Content = JsonContent.Create(body);
            }

            return await client.SendAsync(request);
        }

        // Helper to extract the API cookie from a login response
        public static string? ExtractSessionCookie(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("Set-Cookie", out var values))
            {
                var sessionHeader = values.FirstOrDefault(v => v.StartsWith(".AspNetCore.Session=", StringComparison.OrdinalIgnoreCase));
                return sessionHeader?.Split(';', 2)[0]; // Returns only "name=value"
            }
            return null;
        }

        public async Task<T?> ReadAsync<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) return default;
            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<HttpResponseMessage> UploadFileAsync(string url, IFormFile file)
        {
            var client = _factory.CreateClient("RaceDayApi");
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            string? apiCookie = _httpContextAccessor.HttpContext?.Session.GetString("ApiSessionCookie");
            if (!string.IsNullOrWhiteSpace(apiCookie))
                request.Headers.TryAddWithoutValidation("Cookie", apiCookie);

            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            request.Content = content;
            return await client.SendAsync(request);
        }
        

    }
}