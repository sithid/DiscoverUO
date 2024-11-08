using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverUO.Lib.DTOs
{
    public class UpdateUserPasswordDto
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
