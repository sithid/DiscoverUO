namespace DiscoverUO.Lib.DTOs.Users
{
    public class UserDto
    {
        public string? UserName { get; set; }
        public int DailyVotesRemaining { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int? ProfileId { get; set; }
        public int? FavoritesId { get; set; }
    }
}