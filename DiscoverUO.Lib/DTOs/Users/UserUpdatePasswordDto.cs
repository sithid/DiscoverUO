using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverUO.Lib.DTOs.Users
{
    public class UserUpdatePasswordDto
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
