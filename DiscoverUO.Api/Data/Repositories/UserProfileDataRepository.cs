using DiscoverUO.Api.Data.Repositories.Contracts;
using DiscoverUO.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscoverUO.Api.Data.Repositories
{
    public class UserProfileDataRepository : IUserProfileDataRepository
    {
        private readonly DiscoverUODatabaseContext _context;

        public UserProfileDataRepository(DiscoverUODatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<UserProfile>> GetUserProfiles()
        {
            var userProfiles = await _context.UserProfiles.ToListAsync();

            return userProfiles;
        }

        public async Task<UserProfile> GetUserProfile(int id)
        {
            var user = await _context.UserProfiles.FindAsync(id);

            return user;
        }

        public async Task<bool> PutUserProfile(UserProfile userProfile)
        {
            _context.Entry(userProfile).State = EntityState.Modified;

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

        public async Task<bool> PostUserProfile(UserProfile userProfile)
        {
            _context.UserProfiles.Add(userProfile);

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

        public async Task<bool> DeleteUserProfile(int id)
        {
            var userProfile = await _context.UserProfiles.FindAsync(id);

            if (userProfile != null)
            {
                _context.UserProfiles.Remove(userProfile);

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

        public async Task<bool> UserProfileExists(int id)
        {
            return _context.UserProfiles.Any(user => user.Id == id);
        }
    }
}
