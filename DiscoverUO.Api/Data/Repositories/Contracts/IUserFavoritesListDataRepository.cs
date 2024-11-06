using DiscoverUO.Api.Models;
using DiscoverUO.Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;

namespace DiscoverUO.Api.Data.Repositories.Contracts
{
    public interface IUserFavoritesListDataRepository
    {
        Task<List<UserFavoritesList>> GetUserFavoritesLists();
        Task<UserFavoritesList> GetUserFavoritesList(int id);
        Task<bool> PutUserFavoritesList(UserFavoritesList favoritesList);
        Task<bool> PostUserFavoritesList(UserFavoritesList favoritesList);
        Task<bool> DeleteUserFavoritesList(int id);
        Task<bool> UserFavoritesListExists(int id);
    }
}