using System.Net;
using DiscoverUO.Lib.Shared.Contracts;

namespace DiscoverUO.Lib.Shared.Favorites
{
    public class FavoriteItemDataReponse : IEntityResponse<FavoriteItemData>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public FavoriteItemData Entity { get; set; }
    }
}
