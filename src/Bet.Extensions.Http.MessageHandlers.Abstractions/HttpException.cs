using System;
using System.Net;

namespace Bet.Extensions.Http.MessageHandlers.Abstractions
{
    public class HttpException : Exception
    {
        public HttpException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpException() : base()
        {
        }

        public HttpException(string message) : base(message)
        {
        }

        public HttpException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public HttpStatusCode StatusCode { get; }
    }
}
