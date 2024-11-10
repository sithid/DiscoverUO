namespace DiscoverUO.Lib.DTOs.Users
{
    public enum UserRole
    {
        BasicUser,
        AdvancedUser,
        Moderator,
        Admin,
        Owner
    }

    public class UserDto
    {
        public string? UserName { get; set; }
        public int DailyVotesRemaining { get; set; }
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public int? ProfileId { get; set; }
        public int? FavoritesId { get; set; }
    }
}