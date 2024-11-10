using System.Text.Json.Serialization;

namespace DiscoverUO.Api.Models
{
    public class UserFavoritesListItem
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string? ServerName { get; set; }
        public string? ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public string? ServerEra { get; set; }
        public bool PvPEnabled { get; set; }

        public int FavoritesListId { get; set; }
        [JsonIgnore]
        public UserFavoritesList? FavoritesList { get; set; }

    }
}