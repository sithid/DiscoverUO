using System.Net;
using DiscoverUO.Lib.Shared.Contracts;

namespace DiscoverUO.Lib.Shared.Favorites
{
    public class FavoritesDataListReponse : IEntityResponse<FavoritesData>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public FavoritesData Entity { get; set; }
    }
}
