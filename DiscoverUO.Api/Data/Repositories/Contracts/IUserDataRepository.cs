using DiscoverUO.Api.Models;

namespace DiscoverUO.Api.Data.Repositories.Contracts
{
    public interface IUserDataRepository
    {
        Task<List<User>> GetUsers();
        Task<User> GetUser(int id);
        Task<bool> PutUser(User user);
        Task<bool> PostUser(User user);
        Task<bool> DeleteUser(int id);
        Task<bool> DeleteUser(User user);
        Task<bool> UserExists(int id);
    }
}
