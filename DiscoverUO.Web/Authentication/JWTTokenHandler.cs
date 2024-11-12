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

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _localStorage.GetItemAsync<string>("jwtToken").Result;

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token );
            }

            return base.SendAsync(request, cancellationToken).Result;
        }
    }
}