using DiscoverUO.Lib.DTOs.Favorites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverUO.Lib.DTOs
{
    public class DashboardDto
    {
        public string? Username { get; set; }
        public int DailyVotesRemaining { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? UserBiography { get; set; }
        public string? UserDisplayName { get; set; }
        public string? UserAvatar { get; set; }
        public UserFavoritesListDto Favorites { get; set; }
    }
}
