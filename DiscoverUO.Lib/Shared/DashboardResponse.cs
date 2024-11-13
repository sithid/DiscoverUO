using DiscoverUO.Lib.DTOs.Users;
using DiscoverUO.Lib.Shared.Contracts;
using System.Net;

namespace DiscoverUO.Lib.Shared
{
	public class DashboardResponse : IDataResponse<DashboardDto>
	{
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public DashboardDto Data { get; set; }
    }
}
