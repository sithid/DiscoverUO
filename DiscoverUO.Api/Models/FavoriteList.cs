namespace DiscoverUO.Api.Models
{
    public class FavoriteList
    {
        public int Id { get; set; }
        public int UserId {  get; set; }
        public string ListName {  get; set; }
        public bool Public { get; set; }
        public List<Server>? FavoritedServers { get; set; } // ServerId
    }
}