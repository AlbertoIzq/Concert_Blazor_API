using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Concert.UIWasm.Data
{
    public class WebApiExecuter : IWebApiExecuter
    {
        private readonly HttpClient _httpClient;

        public WebApiExecuter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task InvokePost<T>(string relativeUrl, T obj)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, relativeUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json")
            };
            // You could also do: var httpResponse = await _httpClient.PostAsJsonAsync(relativeUrl, obj);
            var httpResponse = await _httpClient.SendAsync(httpRequest);

            await HandlePotentialError(httpResponse);
        }

        public async Task<T?> InvokeGet<T>(string relativeUrl)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
            var httpResponse = await _httpClient.SendAsync(httpRequest);

            await HandlePotentialError(httpResponse);

            return await httpResponse.Content.ReadFromJsonAsync<T>();
        }

        public async Task InvokePut<T>(string relativeUrl, T obj)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Put, relativeUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json")
            };
            // You could also do: var httpResponse = await _httpClient.PutAsJsonAsync(relativeUrl, obj);
            var httpResponse = await _httpClient.SendAsync(httpRequest);

            await HandlePotentialError(httpResponse);
        }

        public async Task InvokeDelete(string relativeUrl)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, relativeUrl);
            var httpResponse = await _httpClient.SendAsync(httpRequest);

            await HandlePotentialError(httpResponse);
        }

        private async Task HandlePotentialError(HttpResponseMessage httpResponse)
        {
            if (!httpResponse.IsSuccessStatusCode)
            {
                // Log or handle non-success status codes
                var errorJson = await httpResponse.Content.ReadAsStringAsync();
                throw new WebApiException(errorJson);
            }
        }
    }
}