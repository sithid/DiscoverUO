using Blazored.LocalStorage;
using DiscoverUO.Lib.Shared.Servers;

namespace DiscoverUO.Web.Components.Data
{
    public static class ServerListManager
    {      
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
