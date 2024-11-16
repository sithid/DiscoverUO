using DiscoverUO.Lib.Shared.Users;

namespace DiscoverUO.Web.Data
{
    public class SessionState
    {
        public bool IsLoggedIn {  get; set; }
        public GetDashboardRequest UserDashboardData { get; set; }
    }
}
