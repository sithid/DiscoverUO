using DiscoverUO.Api.Models;
using DiscoverUO.Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;

namespace DiscoverUO.Api.Data.Repositories.Contracts
{
    public interface IUserProfileDataRepository
    {
        Task<List<UserProfile>> GetUserProfiles();
        Task<UserProfile> GetUserProfile(int id);
        Task<bool> PutUserProfile(UserProfile userProfile);
        Task<bool> PostUserProfile(UserProfile userProfile);
        Task<bool> DeleteUserProfile(int id);
        Task<bool> UserProfileExists(int id);
    }
}