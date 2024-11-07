using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverUO.Lib.DTOs
{
    public class UserFavoritesListDto
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public List<int>? FavoriteIds { get; set; }
    }
}
