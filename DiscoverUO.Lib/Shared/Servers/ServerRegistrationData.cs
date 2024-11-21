namespace DiscoverUO.Lib.Shared.Servers
{
    public class ServerRegistrationData
    {
        public string? ServerName { get; set; }
        public string? ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public string? ServerEra { get; set; }
        public string? ServerWebsite { get; set; }
        public string? ServerBanner { get; set; }
        public bool PvPEnabled { get; set; }
        public bool IsPublic { get; set; }
    }
}
