using DiscoverUO.Lib.Shared.Contracts;
using System.Net;

namespace DiscoverUO.Lib.Shared.Users
{
    public class AuthenticationResponse : IEntityResponse<IdentityData>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public IdentityData Entity { get; set; }
    }
}
