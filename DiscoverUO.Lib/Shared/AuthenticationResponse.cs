using DiscoverUO.Lib.DTOs.Users;
using DiscoverUO.Lib.Shared.Contracts;
using System.Net;

namespace DiscoverUO.Lib.Shared
{
    public class AuthenticationResponse : IDataResponse<string>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Data { get; set; }
    }
}
