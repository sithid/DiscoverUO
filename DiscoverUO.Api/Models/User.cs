namespace DiscoverUO.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? DailyVotesRemaining { get; set; }
        public UserProfile? UserProfile { get; set; }
        public FavoriteList? FavoriteServersList { get; set; }
    }
}
