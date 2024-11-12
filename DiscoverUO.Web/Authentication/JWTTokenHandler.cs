using Blazored.LocalStorage;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;

namespace DiscoverUO.Web.Authentication
{
    public class JWTTokenHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;

        public JWTTokenHandler(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _localStorage.GetItemAsync<string>("jwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}