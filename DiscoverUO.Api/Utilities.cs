using DiscoverUO.Api.Models;
using System.Security.Claims;
using DiscoverUO.Lib.DTOs.Users;

namespace DiscoverUO.Api
{
    public static class Permissions
    {
        internal static readonly UserRole[] StandardPermissions = { UserRole.BasicUser, UserRole.AdvancedUser };
        internal static readonly UserRole[] ElevatedPermissions = { UserRole.Moderator, UserRole.Admin, UserRole.Owner };
        internal static readonly UserRole[] AllPermissions = StandardPermissions.Concat(ElevatedPermissions).ToArray();
        internal static bool HasValidRole(UserRole role)
        {
            return AllPermissions.Contains(role);
        }
        internal static bool HasStandardRole(UserRole role)
        {
            return StandardPermissions.Contains(role);
        }
        internal static bool HasElevatedRole(UserRole role)
        {
            return ElevatedPermissions.Contains(role);
        }
        internal static bool HasHigherPermission(UserRole updaterRole, UserRole updatedRole)
        {
            return updaterRole > updatedRole;
        }
        internal static bool HasServerPermissions( User user, Server server)
        {
            return 
                (user.Id == server.OwnerId) ||
                HasElevatedRole(user.Role);
        }
        internal static async Task<int> GetCurrentUserId( ClaimsPrincipal user)
        {
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return 0;
            }

            var userId = int.Parse(userIdClaim.Value);

            return userId;
        }
        internal static UserRole GetCurrentUserRole( ClaimsPrincipal user )
        {
            var userRoleClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            if (userRoleClaim == null || !Enum.TryParse(userRoleClaim.Value, out UserRole role) || !HasValidRole(role))
            {
                return UserRole.BasicUser; // Default to BasicUser
            }

            return role;
        }
    }
}