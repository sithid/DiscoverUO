﻿using DiscoverUO.Lib.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverUO.Lib.Shared.Users
{
    public class DeleteUserResponse : IValueResponse<int>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public int Value { get; set; }
    }
}
