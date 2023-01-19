using System.Text;

namespace Bet.Extensions.Resilience.Http.Abstractions.Options;

public class HttpClientBasicAuthOptions : HttpClientOptions
{
    private string _password = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password
    {
        get => _password.FromBase64String();
        set => _password = value;
    }

    public string GetBasicAuthorizationHeaderValue()
    {
        if (string.IsNullOrEmpty(Username))
        {
            throw new ArgumentNullException("IsNullOrEmpty", nameof(Username));
        }

        if (string.IsNullOrEmpty(Password))
        {
            throw new ArgumentNullException("IsNullOrEmpty", nameof(Password));
        }

        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"));
    }
}
