using DiscoverUO.Lib.Shared.Contracts;
using System.Net;

namespace DiscoverUO.Lib.Shared.Users
{
    public class DashboardResponse : IEntityResponse<DashboardRequest>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public DashboardRequest Entity { get; set; }
    }
}