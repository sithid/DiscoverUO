using System.Text.Json.Serialization;

namespace DiscoverUO.Api.Models
{
    public class UserFavoritesListItem
    {
        public int Id { get; set; }
        public string Name {  get; set; }
        public int FavoritesListId { get; set; }

        [JsonIgnore]
        public UserFavoritesList FavoritesList { get; set; }

    }
}