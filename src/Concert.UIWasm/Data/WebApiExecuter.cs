using System.Net.Http.Json;

namespace Concert.UIWasm.Data
{
    public class WebApiExecuter : IWebApiExecuter
    {
        private readonly HttpClient _httpClient;

        public WebApiExecuter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T?> InvokeGet<T>(string relativeUrl)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
            var httpResponse = await _httpClient.SendAsync(httpRequest);

            await HandlePotentialError(httpResponse);

            return await httpResponse.Content.ReadFromJsonAsync<T>();
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