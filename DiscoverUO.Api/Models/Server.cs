﻿using System.Text.Json.Serialization;

namespace DiscoverUO.Api.Models
{
    public class Server
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string? ServerName { get; set; }
        public string? ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public string? ServerEra { get; set; }
        public string? ServerWebsite { get; set; }
        public string? ServerBanner { get; set; }
        public bool PvPEnabled { get; set; }
        public bool IsPublic { get; set; }
        public int Votes { get; set; }
        public int Rating { get; set; }
        public string? CreationDate { get; set; }

        [JsonIgnore]
        public User Owner { get; set; }
    }
}