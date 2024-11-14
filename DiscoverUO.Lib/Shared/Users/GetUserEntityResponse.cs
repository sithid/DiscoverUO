using System.Net;
using DiscoverUO.Lib.Shared.Contracts;

namespace DiscoverUO.Lib.Shared.Users
{
    public class GetUserEntityResponse : IEntityResponse<GetUserEntityRequest>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public GetUserEntityRequest Entity { get; set; }
    }
}
