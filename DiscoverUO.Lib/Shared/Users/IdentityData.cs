using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverUO.Lib.Shared.Users
{
    public class IdentityData
    {
        public string Username {  get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string SecurityToken { get; set; } = string.Empty;
    }
}
