﻿using DiscoverUO.Lib.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverUO.Lib.Shared.Data
{
    public class ExceptionThrownResponse : IResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;
        public Exception Exception { get; set; }
    }
}
