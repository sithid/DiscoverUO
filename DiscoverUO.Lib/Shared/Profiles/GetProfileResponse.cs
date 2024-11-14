using DiscoverUO.Lib.Shared.Contracts;
using System.Net;

namespace DiscoverUO.Lib.Shared.Profiles
{
    public class GetProfileResponse : IEntityResponse<GetProfileRequest>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public GetProfileRequest Entity { get; set; }
    }
}
