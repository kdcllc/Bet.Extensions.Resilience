using System;
using System.Text;

namespace Bet.Extensions.MessageHandlers
{
    public class HttpBasicAuthClientOptions : HttpClientOptions
    {
        private string _password;

        public string Username { get; set; }

        public string Password
        {
            get => _password.FromBase64String();
            set => _password = value;
        }

        public string GetBasicAuthorizationHeaderValue()
        {
            if (string.IsNullOrEmpty(Username))
            {
                throw new ArgumentNullException(nameof(Username));
            }

            if (string.IsNullOrEmpty(Password))
            {
                throw new ArgumentNullException(nameof(Password));
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"));
        }
    }
}
