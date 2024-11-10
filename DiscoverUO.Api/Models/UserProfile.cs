using System.Text.Json.Serialization;

namespace DiscoverUO.Api.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string? UserBiography { get; set; }
        public string? UserDisplayName { get; set; }
        public string? UserAvatar { get; set; }

        public int OwnerId { get; set; }
        [JsonIgnore]
        public User Owner { get; set; }
    }
}
