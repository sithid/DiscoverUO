using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverUO.Lib.DTOs.Servers
{
    public class ServerUpdateDto
    {
        public string? ServerName { get; set; }
        public string? ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public string? ServerEra { get; set; }
        public bool PvPEnabled { get; set; }
        public bool IsPublic { get; set; }
    }
}
