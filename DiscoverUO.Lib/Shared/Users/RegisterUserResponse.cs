﻿using DiscoverUO.Lib.Shared.Contracts;
using System.Net;

namespace DiscoverUO.Lib.Shared.Users
{
    public class RegisterUserResponse : IEntityResponse<UserEntityData>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public UserEntityData Entity { get; set; }
    }
}
