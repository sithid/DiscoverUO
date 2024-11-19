namespace DiscoverUO.Lib.Shared.Servers
{
    public class ServerData
    {
        public string? ServerName { get; set; }
        public string? ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public string? ServerEra { get; set; }
        public bool PvPEnabled { get; set; }
        public bool IsPublic { get; set; }
        public int Votes { get; set; }
        public int Rating { get; set; }
    }
}
