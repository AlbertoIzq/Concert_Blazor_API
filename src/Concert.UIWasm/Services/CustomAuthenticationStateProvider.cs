using Concert.Business.Models.Domain;
using Concert.UIWasm.Data;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Concert.UIWasm.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IWebApiExecuter _webApiExecuter;

        public CustomAuthenticationStateProvider(IWebApiExecuter webApiExecuter)
        {
           _webApiExecuter = webApiExecuter;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userInfo = await _webApiExecuter.InvokeGet<UserInfoResponseDto>("auth/user-info");

                if (userInfo is not null)
                {
                    var listClaims = new List<Claim>();
                    userInfo.Claims.ForEach(c => listClaims.Add(new Claim(c.Type, c.Value)));
                    var identity = new ClaimsIdentity(listClaims, "jwt");
                    var user = new ClaimsPrincipal(identity);
                    
                    return new AuthenticationState(user);
                }
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            catch (Exception ex)
            {
                // User is not authenticated
            }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        
        public async Task<HttpResponseMessage> LoginAsync(LoginRequestDto loginRequestDto)
        {
            var response = await _webApiExecuter.InvokePostWithResponse("auth/login", loginRequestDto);

            if (response.IsSuccessStatusCode)
            {
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }

            return response;
        }
        
        public async Task LogoutAsync()
        {
            await _webApiExecuter.InvokePost("auth/logout", new object());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}