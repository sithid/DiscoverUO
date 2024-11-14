using DiscoverUO.Lib.DTOs.Users;
using DiscoverUO.Lib.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverUO.Lib.Shared
{
    public class UserResponse : IDataResponse<UserRequest>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public UserRequest Data { get; set; }
    }
}
