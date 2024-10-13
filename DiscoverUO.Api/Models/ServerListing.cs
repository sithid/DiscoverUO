using System.Diagnostics;

namespace DiscoverUO.Api.Models
{
    public enum GameEra
    {
        PreAos = 1,
        Aos = 2
    };

    public class ServerListing
    {
        public int UserId {  get; set; }
        public int ServerId { get; set; }
        public string ServerName { get; set; }
        public GameEra ServerEra { get; set; }
        public string ServerAddress {  get; set; }
        public int ServerPort {  get; set; }
        public bool IsPublic {  get; set; }
        public int Votes {  get; set; }
        public int Rating {  get; set; }
    }
}