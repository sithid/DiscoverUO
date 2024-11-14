using System.Net;
using DiscoverUO.Lib.Shared.Users;
using DiscoverUO.Lib.Shared.Contracts;

namespace DiscoverUO.Lib.Shared.Users
{
    public class UserTableResponse : IListResponse<GetUserRequest>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<GetUserRequest> Table { get; set; }
    }
}
