using System.Text.Json.Serialization;

namespace DiscoverUO.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? PasswordHash { get; set; }
        public string? Email { get; set; }
        public string? Role {  get; set; }
        public int DailyVotesRemaining { get; set; }
        public ICollection<Server>? ServersAdded { get; set; }
        public UserProfile? Profile { get; set; }
        public UserFavoritesList? Favorites { get; set; }
    }
}
