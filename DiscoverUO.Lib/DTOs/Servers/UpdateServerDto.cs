namespace DiscoverUO.Lib.DTOs.Servers
{
    public class UpdateServerDto
    {
        public string? ServerName { get; set; }
        public string? ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public string? ServerEra { get; set; }
        public bool PvPEnabled { get; set; }
        public bool IsPublic { get; set; }
    }
}
