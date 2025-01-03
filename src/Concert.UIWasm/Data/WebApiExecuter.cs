using Concert.UIWasm.Helpers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Concert.UIWasm.Data
{
    public class WebApiExecuter : IWebApiExecuter
    {
        private readonly HttpClient _httpClient;
        private readonly string HttpRequestExceptionMessage = "Unable to reach the server. Please try again later.";
        private readonly string WebApiExceptionMessage = "An unexpected error occurred, the API request was unsuccessful.";
        private readonly string ExceptionMessage = "An unexpected error occurred.";

        public WebApiExecuter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task InvokePost<T>(string relativeUrl, T obj)
        {
            var httpMethod = HttpMethod.Post;

            try
            {
                var httpRequest = new HttpRequestMessage(httpMethod, relativeUrl)
                {
                    Content = new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json")
                };
                // You could also do: var httpResponse = await _httpClient.PostAsJsonAsync(relativeUrl, obj);
                var httpResponse = await _httpClient.SendAsync(httpRequest);

                await HandleUnsuccessfulStatus(httpResponse);
            }
            catch (HttpRequestException ex)
            {
                LoggerHelper.LogException(HttpRequestExceptionMessage, ex);
                throw new Exception(HttpRequestExceptionMessage, ex);
            }
            catch (WebApiException ex)
            {
                string message = ex.ProblemDetails?.Title ?? WebApiExceptionMessage;
                int status = ex.ProblemDetails?.Status ?? 0;
                LoggerHelper.LogWebApiException(message, relativeUrl, httpMethod, status, ex);
                throw new Exception(message, ex);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogException(ExceptionMessage, ex);
                throw new Exception(ExceptionMessage, ex);
            }
        }

        public async Task<T?> InvokeGet<T>(string relativeUrl)
        {
            var httpMethod = HttpMethod.Get;

            try
            {
                var httpRequest = new HttpRequestMessage(httpMethod, relativeUrl);
                var httpResponse = await _httpClient.SendAsync(httpRequest);

                await HandleUnsuccessfulStatus(httpResponse);

                return await httpResponse.Content.ReadFromJsonAsync<T>();
            }
            catch (HttpRequestException ex)
            {
                LoggerHelper.LogException(HttpRequestExceptionMessage, ex);
                throw new Exception(HttpRequestExceptionMessage, ex);
            }
            catch (WebApiException ex)
            {
                string message = ex.ProblemDetails?.Title ?? WebApiExceptionMessage;
                int status = ex.ProblemDetails?.Status ?? 0;
                LoggerHelper.LogWebApiException(message, relativeUrl, httpMethod, status, ex);
                throw new Exception(message, ex);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogException(ExceptionMessage, ex);
                throw new Exception(ExceptionMessage, ex);
            }
        }

        public async Task InvokePut<T>(string relativeUrl, T obj)
        {
            var httpMethod = HttpMethod.Put;

            try
            {
                var httpRequest = new HttpRequestMessage(httpMethod, relativeUrl)
                {
                    Content = new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json")
                };
                // You could also do: var httpResponse = await _httpClient.PutAsJsonAsync(relativeUrl, obj);
                var httpResponse = await _httpClient.SendAsync(httpRequest);

                await HandleUnsuccessfulStatus(httpResponse);
            }
            catch (HttpRequestException ex)
            {
                LoggerHelper.LogException(HttpRequestExceptionMessage, ex);
                throw new Exception(HttpRequestExceptionMessage, ex);
            }
            catch (WebApiException ex)
            {
                string message = ex.ProblemDetails?.Title ?? WebApiExceptionMessage;
                int status = ex.ProblemDetails?.Status ?? 0;
                LoggerHelper.LogWebApiException(message, relativeUrl, httpMethod, status, ex);
                throw new Exception(message, ex);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogException(ExceptionMessage, ex);
                throw new Exception(ExceptionMessage, ex);
            }
        }

        public async Task InvokeDelete(string relativeUrl)
        {
            var httpMethod = HttpMethod.Delete;

            try
            {
                var httpRequest = new HttpRequestMessage(httpMethod, relativeUrl);
                var httpResponse = await _httpClient.SendAsync(httpRequest);

                await HandleUnsuccessfulStatus(httpResponse);
            }
            catch (HttpRequestException ex)
            {
                LoggerHelper.LogException(HttpRequestExceptionMessage, ex);
                throw new Exception(HttpRequestExceptionMessage, ex);
            }
            catch (WebApiException ex)
            {
                string message = ex.ProblemDetails?.Title ?? WebApiExceptionMessage;
                int status = ex.ProblemDetails?.Status ?? 0;
                LoggerHelper.LogWebApiException(message, relativeUrl, httpMethod, status, ex);
                throw new Exception(message, ex);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogException(ExceptionMessage, ex);
                throw new Exception(ExceptionMessage, ex);
            }
        }

        private async Task HandleUnsuccessfulStatus(HttpResponseMessage httpResponse)
        {
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorJson = await httpResponse.Content.ReadAsStringAsync();
                throw new WebApiException(errorJson);
            }
        }
    }
}