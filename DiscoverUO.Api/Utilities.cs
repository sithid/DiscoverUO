using DiscoverUO.Api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Security;
using System.Security.Claims;

namespace DiscoverUO.Api
{
    public static class UserUtilities
    {
        internal static readonly string[] StandardPermissions = { "BasicUser", "AdvancedUser", };
        internal static readonly string[] ElevatedPermissions = { "Moderator", "Admin", "Owner" };
        internal static readonly string[] AllPermissions = UserUtilities.StandardPermissions.Concat(UserUtilities.ElevatedPermissions).ToArray();
        internal static bool HasValidRole(string role)
        {
            return UserUtilities.AllPermissions.Contains(role);
        }
        internal static bool HasStandardRole(string role)
        {
            if ( UserUtilities.StandardPermissions.Contains(role))
                return true;
            else
                return false;
        }
        internal static bool HasElevatedRole(string role)
        {
            if ( UserUtilities.ElevatedPermissions.Contains(role) )
                return true;
            else
                return false;
        }
        internal static bool HasHigherPermission( string updaterRole, string updatedRole )
        {
            int currentUserRoleIndex = 0;
            int userToUpdateRoleIndex = 0;

            for (int i = 0; i < UserUtilities.AllPermissions.Count(); ++i)
            {
                if (string.Equals(updaterRole, UserUtilities.AllPermissions[i]))
                {
                    currentUserRoleIndex = i;
                }

                if (string.Equals(updatedRole, UserUtilities.AllPermissions[i]))
                {
                    userToUpdateRoleIndex = i;
                }
            }

            if (currentUserRoleIndex >= userToUpdateRoleIndex)
                return true;

            return false;
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
        internal static async Task<string> GetCurrentUserRole( ClaimsPrincipal user)
        {
            var userRoleClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            if (userRoleClaim == null || !HasValidRole(userRoleClaim.Value))
            {
                return "BasicUser";
            }

            return userRoleClaim.Value;
        }
    }
}