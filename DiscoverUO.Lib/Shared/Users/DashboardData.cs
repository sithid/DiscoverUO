using DiscoverUO.Lib.Shared.Favorites;

namespace DiscoverUO.Lib.Shared.Users
{
    public class DashboardData
    {
        public string? Username { get; set; }
        public int DailyVotesRemaining { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? UserBiography { get; set; }
        public string? UserDisplayName { get; set; }
        public string? UserAvatar { get; set; }
        public GetFavoritesRequest Favorites { get; set; }
    }
}
