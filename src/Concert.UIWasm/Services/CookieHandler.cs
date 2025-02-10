using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Concert.UIWasm.Services
{
    public class CookieHandler : DelegatingHandler
    {
        public CookieHandler()
        {
            InnerHandler = new HttpClientHandler();
        }

        /// <summary>
        /// To add cookies in the HTTP request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            return base.SendAsync(request, cancellationToken);
        }
    }
}