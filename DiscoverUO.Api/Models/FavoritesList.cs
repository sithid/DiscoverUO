namespace DiscoverUO.Api.Models
{
    public class FavoritesList
    {
        public int UserId {  get; set; }
        public int ListId { get; set; }
        public string ListName {  get; set; }
        public bool Public { get; set; }
        public List<ServerListing> Servers { get; set; }
    }
}