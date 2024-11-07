using System.Text.Json.Serialization;

namespace DiscoverUO.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? DailyVotesRemaining { get; set; }

        public ICollection<Server> ServersAdded { get; set; }
        public UserProfile Profile { get; set; }
        public UserFavoritesList FavoritesList { get; set; }
    }
}
