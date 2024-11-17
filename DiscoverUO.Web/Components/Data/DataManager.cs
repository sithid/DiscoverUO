using Blazored.LocalStorage;
using DiscoverUO.Lib.Shared.Users;
using DiscoverUO.Lib.Shared.Favorites;
using DiscoverUO.Web.Components.Pages;
using System.Net.Http.Headers;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DiscoverUO.Web.Components.Data
{
    public static class DataManager
    {

        public static async Task SaveDashboardData(ILocalStorageService local, DashboardRequest data)
        {
            try
            {
                string favsJson = JsonSerializer.Serialize<GetFavoritesRequest>(data.Favorites);
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

        public static async Task<DashboardRequest> LoadDashboardData( ILocalStorageService local )
        {
            DashboardRequest dashboard = new DashboardRequest();

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
                    var favorites = JsonSerializer.Deserialize<GetFavoritesRequest>(favsString);

                    dashboard.Favorites = favorites;
                }
                catch
                {
                    dashboard.Favorites = new GetFavoritesRequest();
                }
            }

            return dashboard;
        }

        public static async Task<DashboardRequest> GetDashboard(HttpClient _client, ILocalStorageService local)
        {
            var dashboard = await LoadDashboardData(local);

            if (string.IsNullOrEmpty(dashboard.Username) || string.Equals(dashboard.Username, "anonymous"))
            {
                var token = await local.GetItemAsync<string>("jwtToken");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = _client.GetAsync("https://localhost:7015/api/users/view/dashboard").Result;

                response.EnsureSuccessStatusCode();

                dashboard = new DashboardRequest();

                try
                {
                    var dashboardResponse = response.Content.ReadFromJsonAsync<DashboardResponse>().Result;

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
    }
}
