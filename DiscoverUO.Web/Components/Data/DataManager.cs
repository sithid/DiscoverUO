using Blazored.LocalStorage;
using DiscoverUO.Lib.Shared.Users;
using DiscoverUO.Lib.Shared.Favorites;
using DiscoverUO.Web.Components.Pages;
using System.Net.Http.Headers;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DiscoverUO.Lib.Shared.Servers;
using DiscoverUO.Lib.Shared.Contracts;

namespace DiscoverUO.Web.Components.Data
{
    public static class DataManager
    {      
        public static async Task<DashboardData> GetDashboard(HttpClient _client, ILocalStorageService local)
        {
            var dashboard = new DashboardData();

            var token = await local.GetItemAsync<string>("jwtToken");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = _client.GetAsync("https://localhost:7015/api/users/view/dashboard").Result;

            if (!response.IsSuccessStatusCode)
            {
                return dashboard;
            }

            try
            {
                var dashboardResponse = response.Content.ReadFromJsonAsync<DashboardDataResponse>().Result;

                if (response.IsSuccessStatusCode)
                {
                    dashboard = dashboardResponse.Entity;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"User dashboard is null: {ex}");
            }

            return dashboard;
        }

        public static async Task<List<ServerData>> GetPublicServers(HttpClient _client, ILocalStorageService local)
        {
            var publicServers = new List<ServerData>();

            var response = _client.GetAsync("https://localhost:7015/api/servers/public").Result;

            try
            {
                var serverListReponse = response.Content.ReadFromJsonAsync<ServerListDataResponse>().Result;

                if (response.IsSuccessStatusCode)
                {
                    publicServers = serverListReponse.List;
                }
            }
            catch( Exception ex )
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            return publicServers;
        }
    }
}
