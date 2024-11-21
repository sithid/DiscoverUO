using System.Net;
using DiscoverUO.Lib.Shared.Contracts;

namespace DiscoverUO.Lib.Shared.Users
{
    public class UserEntityResponse : IEntityResponse<UserEntityData>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public UserEntityData Entity { get; set; }
    }
}
