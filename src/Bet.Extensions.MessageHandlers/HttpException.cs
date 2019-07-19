using System;
using System.Net;

namespace Bet.Extensions.MessageHandlers
{
    public class HttpException : Exception
    {
        public HttpException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; private set; }
    }
}
