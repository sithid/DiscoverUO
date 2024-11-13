using System.Net;

namespace DiscoverUO.Lib.Shared.Contracts
{
	public interface IResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public HttpStatusCode StatusCode { get; set; }
	}
}
