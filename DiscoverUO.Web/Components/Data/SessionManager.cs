using DiscoverUO.Lib.Shared;
using DiscoverUO.Lib.Shared.Favorites;
using DiscoverUO.Lib.Shared.Profiles;
using DiscoverUO.Lib.Shared.Users;
using System.Net.Http.Headers;
using System.Net;
using DiscoverUO.Lib.Shared.Contracts;

namespace DiscoverUO.Web.Components.Data
{
    public class SessionManager
    {
        public string Username {  get; set; }
        public string Email {  get; set; }
        public UserRole Role {  get; set; }
        public ProfileData UserProfile { get; set; }
        public FavoritesData UserFavorites { get; set; }
        public bool UserAuthenticated { get; set; }
        public string SecurityToken { get; set; }
        public int DailyVotesRemaining { get; set; }
        public int ProfileId { get; set; }
        public int FavoritesId { get; set; }
        public string? CreationDate { get; set; }
        public bool Banned { get; set; }

        // Multiple uses for this in the future.  One use would be for data analysis.
        private Dictionary<int, IResponse> ResponseCache { get; set; } = new Dictionary<int, IResponse>();

        public SessionManager()
        {
            SetupAnonymousSession();
        }

        public void SetupAnonymousSession()
        {
            Username = "Anonymous";
            Email = "anon@anon.net";
            Role = UserRole.BasicUser;
            UserAuthenticated = false;
            SecurityToken = string.Empty;
            UserProfile = new ProfileData();
            UserFavorites = new FavoritesData();
            UserFavorites.FavoritedItems = new List<FavoriteItemData>();

            DailyVotesRemaining = 0;
            ProfileId = 0;
            FavoritesId = 0;
        }

        public IResponse SessionSignIn( AuthenticationData data, HttpClient client )
        {
            try
            {
                var httpResponse = client.PostAsJsonAsync("https://localhost:7015/api/users/authenticate", data).Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    var authResponse = httpResponse.Content.ReadFromJsonAsync<AuthenticationResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, authResponse);

                    Role = authResponse.Entity.Role;
                    SecurityToken = authResponse.Entity.SecurityToken;
                    UserAuthenticated = true;

                    var dataUpdateRsp = UpdateMiscData(client);

                    if( !dataUpdateRsp.Success )
                    {
                        Console.WriteLine($"Failed to update misc user data: {Username}");
                        Console.WriteLine($"Response StatusCode: {dataUpdateRsp.StatusCode}");
                        Console.WriteLine($"Response Message: {dataUpdateRsp.Message}");
                    }    

                    var favsRequestResponse = UpdateFavoritesData(client);

                    if (!favsRequestResponse.Success)
                    {
                        Console.WriteLine($"Failed to update user favorites data: {Username}");
                        Console.WriteLine($"Response StatusCode: {favsRequestResponse.StatusCode}");
                        Console.WriteLine($"Response Message: {favsRequestResponse.Message}");
                    }

                    var profRequestResponse = UpdateProfileData(client);

                    if (!profRequestResponse.Success)
                    {
                        Console.WriteLine($"Failed to update user favorites data: {Username}");
                        Console.WriteLine($"Response StatusCode: {profRequestResponse.StatusCode}");
                        Console.WriteLine($"Response Message: {profRequestResponse.Message}");
                    }

                    var miscMes = dataUpdateRsp.Success ? "found" : "not found";
                    var favMes = favsRequestResponse.Success ? "found" : "not found";
                    var profMes = profRequestResponse.Success ? "found" : "not found";

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

        public IResponse UpdateMiscData( HttpClient client )
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SecurityToken);

            var response = client.GetAsync("https://localhost:7015/api/users/view").Result;

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
                Console.WriteLine($"Exception: {ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse UpdateProfileData(HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SecurityToken);

            var response = client.GetAsync("https://localhost:7015/api/users/profiles/view").Result;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var profileResponse = response.Content.ReadFromJsonAsync<ProfileResponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, profileResponse);

                    UserProfile = profileResponse.Entity;

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
                Console.WriteLine($"Exception: {ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse UpdateFavoritesData( HttpClient client )
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SecurityToken);

            var response = client.GetAsync("https://localhost:7015/api/favorites/list/view").Result;

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var favoritesResponse = response.Content.ReadFromJsonAsync<FavoritesDataReponse>().Result;
                    ResponseCache.Add(ResponseCache.Count, favoritesResponse);

                    foreach ( FavoriteItemData item in favoritesResponse.Entity.FavoritedItems )
                    {
                        UserFavorites.FavoritedItems.Add( item );
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
                Console.WriteLine($"Exception: {ex.Message}");

                var exeRsp = new ExceptionThrownResponse
                {
                    Exception = ex,
                    Message = ex.Message,
                };

                ResponseCache.Add(ResponseCache.Count, exeRsp);

                return exeRsp;
            }
        }

        public IResponse AddFavoritesItem( HttpClient client )
        {
            return null;
        }
    }
}
