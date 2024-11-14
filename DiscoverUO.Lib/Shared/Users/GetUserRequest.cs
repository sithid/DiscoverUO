namespace DiscoverUO.Lib.Shared.Users
{
    public enum UserRole
    {
        BasicUser = 1,
        AdvancedUser = 2,
        Moderator = 3,
        Admin = 4,
        Owner = 5
    }

    public class GetUserRequest
    {
        public string? UserName { get; set; }
        public int DailyVotesRemaining { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int? ProfileId { get; set; }
        public int? FavoritesId { get; set; }
    }
}