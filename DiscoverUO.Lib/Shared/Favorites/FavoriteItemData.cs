﻿namespace DiscoverUO.Lib.Shared.Favorites
{
    public class FavoriteItemData
    {
        public int Id { get; set; }
        public int ServerId { get; set; }
        public string? ServerName { get; set; }
        public string? ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public string? ServerEra { get; set; }
        public bool PvPEnabled { get; set; }
        public string? ServerWebsite { get; set; }
        public string? ServerBanner { get; set; }
    }
}
