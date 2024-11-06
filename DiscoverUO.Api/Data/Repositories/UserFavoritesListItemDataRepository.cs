using DiscoverUO.Api.Data.Repositories.Contracts;
using DiscoverUO.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscoverUO.Api.Data.Repositories
{
    public class UserFavoritesListItemDataRepository : IUserFavoritesListItemDataRepository
    {
        private readonly DiscoverUODatabaseContext _context;

        public UserFavoritesListItemDataRepository(DiscoverUODatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<UserFavoritesListItem>> GetUserFavoritesListItems()
        {
            var userFavoritesListItems = await _context.UserFavoritesListItems.ToListAsync();

            return userFavoritesListItems;
        }

        public async Task<UserFavoritesListItem> GetUserFavoritesListItem(int id)
        {
            var userFavoritesListItem = await _context.UserFavoritesListItems.FindAsync(id);

            return userFavoritesListItem;
        }

        public async Task<bool> PutUserFavoritesListItem(UserFavoritesListItem userFavoritesListItem)
        {
            _context.Entry(userFavoritesListItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> PostUserFavoritesListItem(UserFavoritesListItem userFavoritesListItem)
        {
            _context.UserFavoritesListItems.Add(userFavoritesListItem);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteUserFavoritesListItem(int id)
        {
            var userFavoritesList = await _context.UserFavoritesListItems.FindAsync(id);

            if (userFavoritesList != null)
            {
                _context.UserFavoritesListItems.Remove(userFavoritesList);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> UserFavoritesListItemExists(int id)
        {
            return _context.UserFavoritesListItems.Any(user => user.Id == id);
        }
    }
}
