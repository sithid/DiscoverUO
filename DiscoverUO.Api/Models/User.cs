namespace DiscoverUO.Api.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DailyVotesRemaining { get; set; }
        public UserProfile Profile { get; set; }
        public List<FavoritesList> Favorites { get; set; }
        public Dictionary<int, ServerListing> ServersAdded { get; set; }
    }
}
