namespace DiscoverUO.Lib.Shared.Favorites
{
    public class GetFavoriteItemRequest
    {
        public string? ServerName { get; set; }
        public string? ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public string? ServerEra { get; set; }
        public bool PvPEnabled { get; set; }
    }
}
