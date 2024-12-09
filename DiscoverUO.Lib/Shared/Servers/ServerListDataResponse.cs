using System.Net;
using DiscoverUO.Lib.Shared.Contracts;

namespace DiscoverUO.Lib.Shared.Servers
{
    public class ServerListDataResponse : IListResponse<ServerData>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<ServerData> List { get; set; }
    }
}
