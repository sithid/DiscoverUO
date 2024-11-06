using DiscoverUO.Api.Data.Repositories.Contracts;
using DiscoverUO.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscoverUO.Api.Data.Repositories
{
    public class UserDataRepository : IUserDataRepository
    {
        private readonly DiscoverUODatabaseContext _context;

        public UserDataRepository(DiscoverUODatabaseContext context)
        {
            _context = _context;
        }

        public async Task<List<User>> GetUsers()
        {
            var userList = await _context.Users.ToListAsync();

            return userList;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            return user;
        }

        public async Task<bool> PutUser(User user)
        {
            _context.Entry(user).State = EntityState.Modified;

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

        public async Task<bool> PostUser(User user)
        {
            _context.Add(user);

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

        public async Task<bool> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            _context.Users.Remove(user);

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

        public async Task<bool> DeleteUser(User user)
        {
            _context.Users.Remove(user);

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

        public async Task<bool> UserExists(int id)
        {
            return _context.Users.Any( user => user.Id == id);
        }
    }
}
