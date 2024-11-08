using System.Text.Json.Serialization;

namespace DiscoverUO.Lib.DTOs.Users
{
    public class UserDto
    {
        // I havn't decided if I want this exposed.
        [JsonIgnore]
        public int Id { get; set; }

        public string? UserName { get; set; }
        public int DailyVotesRemaining { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public List<int>? ServersAddedIds { get; set; }
        public int? ProfileId { get; set; }
        public int? FavoritesId { get; set; }
    }
}