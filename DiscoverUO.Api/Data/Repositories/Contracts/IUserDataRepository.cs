using DiscoverUO.Api.Models;
using DiscoverUO.Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;

namespace DiscoverUO.Api.Data.Repositories.Contracts
{
    public interface IUserDataRepository
    {
        Task<List<User>> GetUsers();
        Task<User> GetUser(int id);
        Task<bool> PutUser(User user);
        Task<bool> PostUser(User user);
        Task<bool> DeleteUser(int id);
        Task<bool> UserExists(int id);
    }
}