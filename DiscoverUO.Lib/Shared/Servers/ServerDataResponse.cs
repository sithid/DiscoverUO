using System.Net;
using DiscoverUO.Lib.Shared.Users;
using DiscoverUO.Lib.Shared.Contracts;
using DiscoverUO.Lib.Shared.Servers;

namespace DiscoverUO.Lib.Shared.Users
{
    public class ServerDataResponse : IListResponse<ServerData>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<ServerData> List { get; set; }
    }
}
