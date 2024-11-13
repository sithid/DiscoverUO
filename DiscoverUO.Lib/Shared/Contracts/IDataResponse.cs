using DiscoverUO.Lib.DTOs.Users;
using System.Net;


namespace DiscoverUO.Lib.Shared.Contracts
{
	public interface IDataResponse<T> : IResponse where T : DashboardDto
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public T Data { get; set; }
	}
}
