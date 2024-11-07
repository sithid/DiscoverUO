using System.Text.Json.Serialization;

namespace DiscoverUO.Api.Models
{
    public class UserFavoritesList
    {
        public int Id { get; set; }
        public ICollection<UserFavoritesListItem> Favorites { get; set; }

        public int UserId {  get; set; }
        [JsonIgnore]
        public User User { get; set; }
    }
}