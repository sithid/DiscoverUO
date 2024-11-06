using DiscoverUO.Api.Data.Repositories.Contracts;
using DiscoverUO.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscoverUO.Api.Data.Repositories
{
    public class UserFavoritesListDataRepository : IUserFavoritesListDataRepository
    {
        private readonly DiscoverUODatabaseContext _context;

        public UserFavoritesListDataRepository(DiscoverUODatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<UserFavoritesList>> GetUserFavoritesLists()
        {
            var userList = await _context.UserFavoritesLists.ToListAsync();

            return userList;
        }

        public async Task<UserFavoritesList> GetUserFavoritesList(int id)
        {
            var user = await _context.UserFavoritesLists.FindAsync(id);

            return user;
        }

        public async Task<bool> PutUserFavoritesList(UserFavoritesList userFavoritesList)
        {
            _context.Entry(userFavoritesList).State = EntityState.Modified;

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

        public async Task<bool> PostUserFavoritesList(UserFavoritesList userFavoritesList)
        {
            _context.UserFavoritesLists.Add(userFavoritesList);

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

        public async Task<bool> DeleteUserFavoritesList(int id)
        {
            var userFavoritesList = await _context.UserFavoritesLists.FindAsync(id);

            if (userFavoritesList != null)
            {
                _context.UserFavoritesLists.Remove(userFavoritesList);

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

        public async Task<bool> UserFavoritesListExists(int id)
        {
            return _context.UserFavoritesLists.Any(user => user.Id == id);
        }
    }
}
