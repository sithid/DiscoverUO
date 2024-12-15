using System.Net;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using DiscoverUO.Lib.Shared.Users;
using DiscoverUO.Lib.Shared.Servers;
using DiscoverUO.Lib.Shared.Profiles;
using DiscoverUO.Lib.Shared.Contracts;
using DiscoverUO.Lib.Shared.Favorites;

namespace DiscoverUO.Lib.Shared.Data
{
    public class SessionManager
    {
        // Global State Data for the active user.
        public string Username { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public ProfileData UserProfile { get; set; }
        public FavoritesData UserFavorites { get; set; }
        public bool UserAuthenticated { get; set; }
        public string SecurityToken { get; set; }
        public int DailyVotesRemaining { get; set; }
        public int ProfileId { get; set; }
        public int FavoritesId { get; set; }
        public string? CreationDate { get; set; }
        public bool Banned { get; set; }

        public List<ServerData> PublicServers { get; set; }
        public List<ServerData> UserOwnedServers { get; set; }

        public FavoriteItemData UpdateFavoriteTemp { get; set; }
        public ServerUpdateData UpdateServerTmp { get; set; }
        public int UpdateServerId { get; set; }

        // Multiple uses for this in the future.  One use would be for data analysis.
        private Dictionary<int, IResponse> ResponseCache { get; set; } = new Dictionary<int, IResponse>();

        public SessionManager()
        {
            ConfigureAnonymousSession();
        }

        public void ConfigureAnonymousSession()
        {
            Username = "Anonymous";
            Email = "anon@anon.net";
            Role = UserRole.Anonymous;
            UserAuthenticated = false;
            SecurityToken = string.Empty;

            UserProfile = new ProfileData();
            UserFavorites = new FavoritesData();
            UserFavorites.FavoritedItems = new List<FavoriteItemData>();
            UpdateFavoriteTemp = null;

            PublicServers = new List<ServerData>();
            UserOwnedServers = new List<ServerData>();
            UpdateServerTmp = null;

            DailyVotesRemaining = 0;
            ProfileId = 0;
            FavoritesId = 0;
            UpdateServerId = 0;
        }

        public IResponse GetPublicServersList(HttpClient client)
        {
            var response = client.GetAsync("/api/servers/public").Result;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var serverListReponse = response.Content.ReadFromJsonAsync<ServerListDataResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, serverListReponse);

                    PublicServers = serverListReponse.List;

                    return new BasicSuccessResponse
                    {
                        Success = true,
                        Message = serverListReponse.Message,
                        StatusCode = serverListReponse.StatusCode
                    };
                }
                else
                {
                    var failedResponse = response.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, failedResponse);

                    return failedResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was thrown while loading the public server list: {ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse UserSignIn(AuthenticationData data, HttpClient client)
        {
            try
            {
                var httpResponse = client.PostAsJsonAsync("/api/users/authenticate", data).Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    var authResponse = httpResponse.Content.ReadFromJsonAsync<AuthenticationResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, authResponse);

                    Role = authResponse.Entity.Role;
                    SecurityToken = authResponse.Entity.SecurityToken;
                    UserAuthenticated = true;

                    var dataUpdateRsp = GetUserData(client);

                    if (!dataUpdateRsp.Success)
                    {
                        Console.WriteLine($"Failed to update misc user data: {Username}");
                        Console.WriteLine($"Response StatusCode: {dataUpdateRsp.StatusCode}");
                        Console.WriteLine($"Response Message: {dataUpdateRsp.Message}");
                    }

                    var favsRequestResponse = GetUserFavoritesData(client);

                    if (!favsRequestResponse.Success)
                    {
                        Console.WriteLine($"Failed to update user favorites data: {Username}");
                        Console.WriteLine($"Response StatusCode: {favsRequestResponse.StatusCode}");
                        Console.WriteLine($"Response Message: {favsRequestResponse.Message}");
                    }

                    var profRequestResponse = GetUserProfileData(client);

                    if (!profRequestResponse.Success)
                    {
                        Console.WriteLine($"Failed to update user favorites data: {Username}");
                        Console.WriteLine($"Response StatusCode: {profRequestResponse.StatusCode}");
                        Console.WriteLine($"Response Message: {profRequestResponse.Message}");
                    }

                    var ownedServersRequestResponse = GetUserOwnedServers(client);

                    if (!ownedServersRequestResponse.Success)
                    {
                        Console.WriteLine($"Failed to update user favorites data: {Username}");
                        Console.WriteLine($"Response StatusCode: {ownedServersRequestResponse.StatusCode}");
                        Console.WriteLine($"Response Message: {ownedServersRequestResponse.Message}");
                    }

                    var miscMes = dataUpdateRsp.Success ? "found" : "not found";
                    var favMes = favsRequestResponse.Success ? "found" : "not found";
                    var profMes = profRequestResponse.Success ? "found" : "not found";
                    var ownedMes = ownedServersRequestResponse.Success ? "found" : "not found";

                    return new BasicSuccessResponse
                    {
                        Success = true,
                        Message = $"User Data: {miscMes}, Favorites: {favMes}, Profile: {profMes}",
                        StatusCode = HttpStatusCode.OK
                    };
                }
                else
                {
                    var failedAuthResponse = httpResponse.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;

                    return failedAuthResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");

                return new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };
            }
        }

        public IResponse RegisterUser(RegisterUserData newUserData, HttpClient client)
        {
            var registerResponse = client.PostAsJsonAsync("/api/users/register", newUserData).Result;

            if (!registerResponse.IsSuccessStatusCode)
            {
                var failedResponse = registerResponse.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                ResponseCache.Add(ResponseCache.Count, failedResponse);

                return failedResponse;
            }

            Console.WriteLine($"A new user has been registered: Username: {newUserData.UserName}");

            return new BasicSuccessResponse
            {
                Success = true,
                Message = "User registration successfull.",
                StatusCode = registerResponse.StatusCode
            };
        }

        public IResponse GetUserData(HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SecurityToken);

            var response = client.GetAsync("/api/users/view").Result;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var userDataRsp = response.Content.ReadFromJsonAsync<UserEntityResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, userDataRsp);

                    Username = userDataRsp.Entity.UserName;
                    Email = userDataRsp.Entity.Email;
                    DailyVotesRemaining = userDataRsp.Entity.DailyVotesRemaining;
                    ProfileId = userDataRsp.Entity.ProfileId;
                    FavoritesId = userDataRsp.Entity.FavoritesId;
                    CreationDate = userDataRsp.Entity.CreationDate;
                    Banned = userDataRsp.Entity.Banned;

                    return new BasicSuccessResponse
                    {
                        Success = true,
                        Message = userDataRsp.Message,
                        StatusCode = userDataRsp.StatusCode
                    };
                }
                else
                {
                    var failedResponse = response.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, failedResponse);

                    return failedResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was thrown while getting user data.");
                Console.WriteLine($"{ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse GetUserProfileData(HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SecurityToken);

            var response = client.GetAsync("/api/users/profiles/view").Result;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var profileResponse = response.Content.ReadFromJsonAsync<ProfileResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, profileResponse);

                    UserProfile = profileResponse.Entity;
                    ProfileId = profileResponse.Entity.Id;

                    return new BasicSuccessResponse
                    {
                        Success = true,
                        Message = profileResponse.Message,
                        StatusCode = profileResponse.StatusCode
                    };
                }
                else
                {
                    var failedResponse = response.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, failedResponse);

                    return failedResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was thrown while getting user profile data.");
                Console.WriteLine($"{ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse GetUserFavoritesData(HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SecurityToken);

            var response = client.GetAsync("/api/favorites/list/view").Result;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var favoritesResponse = response.Content.ReadFromJsonAsync<FavoritesDataReponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, favoritesResponse);

                    UserFavorites = new FavoritesData();
                    UserFavorites.FavoritedItems = new List<FavoriteItemData>();

                    if (favoritesResponse.Entity.FavoritedItems != null)
                    {
                        if (favoritesResponse.Entity.FavoritedItems.Count > 0)
                        {
                            foreach (FavoriteItemData data in favoritesResponse.Entity.FavoritedItems)
                            {
                                UserFavorites.FavoritedItems.Add(data);
                            }
                        }
                    }

                    return new BasicSuccessResponse
                    {
                        Success = true,
                        Message = favoritesResponse.Message,
                        StatusCode = favoritesResponse.StatusCode
                    };
                }
                else
                {
                    var failedResponse = response.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, failedResponse);

                    return failedResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was thrown while getting user favorites data.");
                Console.WriteLine($"{ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse GetUserOwnedServers( HttpClient client )
        {
            var response = client.GetAsync($"/api/servers/view/owner/{Username}").Result;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var serverListReponse = response.Content.ReadFromJsonAsync<ServerListDataResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, serverListReponse);

                    UserOwnedServers = serverListReponse.List;

                    return new BasicSuccessResponse
                    {
                        Success = true,
                        Message = serverListReponse.Message,
                        StatusCode = serverListReponse.StatusCode
                    };
                }
                else
                {
                    var failedResponse = response.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, failedResponse);

                    return failedResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was thrown while loading user owned servers: {ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse UpdateUserData( UpdateUserData data, HttpClient client)
        {
            var jsonContent = JsonSerializer.Serialize(data);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = $"/api/users/updateuser/{data.UserName}";
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Content = content;

            var response = client.SendAsync(request).Result;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var updateUserRsp = response.Content.ReadFromJsonAsync<BasicSuccessResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, updateUserRsp);

                    var updateUser = GetUserData(client);

                    return updateUserRsp;
                }
                else
                {
                    var failedResponse = response.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, failedResponse);

                    return failedResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was throw while updating user data.");
                Console.WriteLine($"{ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse UpdateUserProfile( ProfileData data, HttpClient client)
        {
            var jsonContent = JsonSerializer.Serialize(data);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = $"/api/users/profiles/update/{data.Id}";
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Content = content;

            var response = client.SendAsync(request).Result;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var updateUserRsp = response.Content.ReadFromJsonAsync<BasicSuccessResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, updateUserRsp);

                    var updateUser = GetUserProfileData(client);

                    return updateUserRsp;
                }
                else
                {
                    var failedResponse = response.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, failedResponse);

                    return failedResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was throw while updating profile data.");
                Console.WriteLine($"{ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse UpdateUserPassword( string username, UpdateUserPasswordData data, HttpClient client)
        {
            var jsonContent = JsonSerializer.Serialize(data);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = $"/api/users/password/update/{username}";
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Content = content;

            var response = client.SendAsync(request).Result;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var updateUserRsp = response.Content.ReadFromJsonAsync<BasicSuccessResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, updateUserRsp);

                    var updateUser = GetUserData(client);

                    return updateUserRsp;
                }
                else
                {
                    var failedResponse = response.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, failedResponse);

                    return failedResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was throw while updating profile data.");
                Console.WriteLine($"{ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse AddServer(ServerRegistrationData data, HttpClient client )
        {
            var createServerRsp = client.PostAsJsonAsync("/api/servers/create_server", data).Result;

            try
            {
                if (createServerRsp.IsSuccessStatusCode)
                {
                    var updatePublicServers = GetPublicServersList(client);
                    var updateUserOwnedServers = GetUserOwnedServers(client);

                    return new BasicSuccessResponse
                    {
                        Success = true,
                        Message = "Server data successfully added.",
                        StatusCode = createServerRsp.StatusCode
                    };
                }
                else
                {
                    var failedResponse = createServerRsp.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, failedResponse);

                    return failedResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was throw while creating a server for the public server list.");
                Console.WriteLine($"{ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse UpdateServer( int serverId, ServerUpdateData data, HttpClient client )
        {
            var jsonContent = JsonSerializer.Serialize(data);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = $"/api/servers/updateserver/{serverId}";
            var request = new HttpRequestMessage(HttpMethod.Put, url);

            request.Content = content;

            var response = client.SendAsync(request).Result;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var favDataRsp = response.Content.ReadFromJsonAsync<BasicSuccessResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, favDataRsp);

                    var updatePublicServers = GetPublicServersList(client);
                    var updateOwnedServers = GetUserOwnedServers(client);

                    UpdateServerTmp = null;
                    UpdateServerId = 0;

                    return favDataRsp;
                }
                else
                {
                    var failedResponse = response.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, failedResponse);

                    return failedResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was throw while adding a favorite server from the public server list.");
                Console.WriteLine($"{ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse RemoveServer( int serverId, ServerData data, HttpClient client )
        {
            string url = $"/api/servers/delete/{serverId}";

            var request = new HttpRequestMessage(HttpMethod.Delete, url);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SecurityToken);

            var response = client.SendAsync(request).Result;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var favDataRsp = response.Content.ReadFromJsonAsync<BasicSuccessResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, favDataRsp);

                    var updatePublicServers = GetPublicServersList(client);
                    var updateUserOwnedServers = GetUserOwnedServers(client);

                    return favDataRsp;
                }
                else
                {
                    var failedResponse = response.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, failedResponse);

                    return failedResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was thrown while deleting server item #{serverId}.");
                Console.WriteLine($"{ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse AddUserFavoritesItem(FavoriteItemData itemData, HttpClient client)
        {
            var addFavRsp = client.PostAsJsonAsync("/api/favorites/list/item/add", itemData).Result;

            try
            {
                if (addFavRsp.IsSuccessStatusCode)
                {
                    var updateFavs = GetUserFavoritesData(client);

                    return new BasicSuccessResponse
                    {
                        Success = true,
                        Message = "Server data successfully added to favorites.",
                        StatusCode = addFavRsp.StatusCode
                    };
                }
                else
                {
                    var failedResponse = addFavRsp.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, failedResponse);

                    return failedResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was throw while adding a favorite server from the public server list.");
                Console.WriteLine($"{ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse UpdateUserFavoritesItem( FavoriteItemData itemData, HttpClient client )
        {
            var jsonContent = JsonSerializer.Serialize(itemData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = $"/api/favorites/list/item/update/{itemData.Id}";
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Content = content;

            var response = client.SendAsync(request).Result;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var favDataRsp = response.Content.ReadFromJsonAsync<BasicSuccessResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, favDataRsp);

                    var updateFavs = GetUserFavoritesData(client);
                    UpdateFavoriteTemp = null;

                    return favDataRsp;
                }
                else
                {
                    var failedResponse = response.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, failedResponse);

                    return failedResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was throw while adding a favorite server from the public server list.");
                Console.WriteLine($"{ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse RemoveItemFromFavorites(int favId, HttpClient client)
        {
            string url = $"/api/favorites/list/item/delete/{favId}";

            var request = new HttpRequestMessage(HttpMethod.Delete, url);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SecurityToken);

            var response = client.SendAsync( request ).Result;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var favDataRsp = response.Content.ReadFromJsonAsync<BasicSuccessResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, favDataRsp);

                    var updateFavs = GetUserFavoritesData( client );

                    return favDataRsp;
                }
                else
                {
                    var failedResponse = response.Content.ReadFromJsonAsync<RequestFailedResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, failedResponse);

                    return failedResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was thrown while deleting favorite item #{favId}.");
                Console.WriteLine($"{ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }
    }
}