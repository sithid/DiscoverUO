using DiscoverUO.Api.Models;
using DiscoverUO.Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;

namespace DiscoverUO.Api.Data.Repositories.Contracts
{
    public interface IUserFavoritesListItemDataRepository
    {
        Task<List<UserFavoritesListItem>> GetUserFavoritesListItems();
        Task<UserFavoritesListItem> GetUserFavoritesListItem(int id);
        Task<bool> PutUserFavoritesListItem(UserFavoritesListItem userFavoritesListItem);
        Task<bool> PostUserFavoritesListItem(UserFavoritesListItem userFavoritesListItem);
        Task<bool> DeleteUserFavoritesListItem(int id);
        Task<bool> UserFavoritesListItemExists(int id);
    }
}