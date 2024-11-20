using Blazored.LocalStorage;
using DiscoverUO.Lib.Shared.Users;
using DiscoverUO.Lib.Shared.Favorites;
using DiscoverUO.Web.Components.Pages;
using System.Net.Http.Headers;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DiscoverUO.Lib.Shared.Servers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DiscoverUO.Web.Components.Data
{
    public static class DataManager
    {

        public static async Task SaveDashboardData(ILocalStorageService local, DashboardData data)
        {
            try
            {
                string favsJson = JsonSerializer.Serialize<FavoritesData>(data.Favorites);
                await local.SetItemAsStringAsync("UserFavorites", favsJson);
            }
            catch( Exception ex )
            {
                await local.SetItemAsStringAsync("UserFavorites", string.Empty);
            }

            await local.SetItemAsStringAsync("Username", data.Username);
            await local.SetItemAsStringAsync("DailyVotesRemaining", data.DailyVotesRemaining.ToString());
            await local.SetItemAsStringAsync("UserDisplayName", data.UserDisplayName);
            await local.SetItemAsStringAsync("UserEmail", data.Email);
            await local.SetItemAsStringAsync("UserRole", data.Role);
            await local.SetItemAsStringAsync("UserBiography", data.UserBiography);
            await local.SetItemAsStringAsync("UserAvatar", data.UserAvatar);
        }

        public static async Task<DashboardData> LoadDashboardData( ILocalStorageService local )
        {
            DashboardData dashboard = new DashboardData();

            dashboard.Username = await local.GetItemAsStringAsync("Username");
            dashboard.UserDisplayName = await local.GetItemAsStringAsync("UserDisplayName");
            dashboard.Email = await local.GetItemAsStringAsync("UserEmail");
            dashboard.Role = await local.GetItemAsStringAsync("UserRole");
            dashboard.UserBiography = await local.GetItemAsStringAsync("UserBiography");
            dashboard.UserAvatar = await local.GetItemAsStringAsync("UserAvatar");

            var votesString = await local.GetItemAsStringAsync("DailyVotesRemaining");

            try
            {
                var votes = Convert.ToInt32(votesString);
                dashboard.DailyVotesRemaining = votes;
            }
            catch (Exception ex)
            {
                dashboard.DailyVotesRemaining = 0;
            }

            var favsString = await local.GetItemAsStringAsync("UserFavorites");

            if (!string.IsNullOrEmpty(favsString))
            {
                try
                {
                    var favorites = JsonSerializer.Deserialize<FavoritesData>(favsString);

                    dashboard.Favorites = favorites;
                }
                catch
                {
                    dashboard.Favorites = new FavoritesData();
                }
            }

            return dashboard;
        }

        public static async Task<DashboardData> GetDashboard(HttpClient _client, ILocalStorageService local)
        {
            var dashboard = await LoadDashboardData(local);

            if (string.IsNullOrEmpty(dashboard.Username) || string.Equals(dashboard.Username, "anonymous"))
            {
                var token = await local.GetItemAsync<string>("jwtToken");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = _client.GetAsync("https://localhost:7015/api/users/view/dashboard").Result;

                if( !response.IsSuccessStatusCode )
                {
                    return new DashboardData();
                }

                dashboard = new DashboardData();

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
                finally
                {
                    SaveDashboardData(local, dashboard);
                }

                return dashboard;
            }

            return dashboard;
        }

        public static async Task<List<ServerData>> GetPublicServers(HttpClient _client, ILocalStorageService local)
        {
            var publicServers = new List<ServerData>();

            var token = await local.GetItemAsync<string>("jwtToken");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = _client.GetAsync("https://localhost:7015/api/servers/public").Result;

            try
            {
                var serverListReponse = response.Content.ReadFromJsonAsync<ServerDataResponse>().Result;

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
