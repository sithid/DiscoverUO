using System.Net;
using DiscoverUO.Lib.Shared.Contracts;

namespace DiscoverUO.Lib.Shared.Favorites
{
    public class FavoritesListReponse : IListResponse<FavoritesData>
    {
        // I would only use this response if pulling the entire table of FavoriteLists.
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<FavoritesData> List { get; set; }
    }
}
