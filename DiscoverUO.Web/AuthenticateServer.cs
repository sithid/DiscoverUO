using DiscoverUO.Lib.DTOs;
using DiscoverUO.Lib.DTOs.Favorites;
using DiscoverUO.Lib.DTOs.Profiles;
using DiscoverUO.Lib.DTOs.Servers;
using DiscoverUO.Lib.DTOs.Users;
using DiscoverUO.Web;
using DiscoverUO.Web.Components;

namespace DiscoverUO.Web.Services.Authentication
{
    public class AuthenticationService
    {
        private readonly HttpClient _httpClient;

        public AuthenticationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> Login( LoginDto loginDto )
        {
            return await _httpClient.GetStringAsync("api/users/Authenticate");
        }
    }
}