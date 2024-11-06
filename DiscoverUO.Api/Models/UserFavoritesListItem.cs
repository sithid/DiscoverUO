namespace DiscoverUO.Api.Models
{
    public class UserFavoritesListItem
    {
        public int Id { get; set; }
        public int ServerId { get; set; }
        public int FavoritesListId { get; set; }
        public string UserFavoritesListItemName {  get; set; }
    }
}