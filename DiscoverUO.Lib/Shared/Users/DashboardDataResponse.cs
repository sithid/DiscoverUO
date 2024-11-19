using DiscoverUO.Lib.Shared.Contracts;
using System.Net;

namespace DiscoverUO.Lib.Shared.Users
{
    public class DashboardDataResponse : IEntityResponse<DashboardData>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public DashboardData Entity { get; set; }
    }
}