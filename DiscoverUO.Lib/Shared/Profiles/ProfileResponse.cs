using DiscoverUO.Lib.Shared.Contracts;
using System.Net;

namespace DiscoverUO.Lib.Shared.Profiles
{
    public class ProfileResponse : IEntityResponse<ProfileData>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public ProfileData Entity { get; set; }
    }
}
