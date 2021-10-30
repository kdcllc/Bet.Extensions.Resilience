using System.Net;

namespace Bet.Extensions.Http.MessageHandlers.Authorize
{
    public class AuthorizeHandlerException : Exception
    {
        public AuthorizeHandlerException() : base()
        {
        }

        public AuthorizeHandlerException(string message) : base(message)
        {
        }

        public AuthorizeHandlerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AuthorizeHandlerException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
