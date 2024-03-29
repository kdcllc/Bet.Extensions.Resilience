﻿using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace Bet.Extensions.Http.MessageHandlers.Authorize;

public class AuthorizeTokenResponse
{
    public AuthorizeTokenResponse()
    {
    }

    public AuthorizeTokenResponse(string accessToken, string refreshToken, string tokenType, int expiresIn)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenType = tokenType;
        ExpiresInSeconds = expiresIn;
    }

    [JsonProperty("access_token")]
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonProperty("refresh_token")]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonProperty("token_type")]
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonProperty("expires_in")]
    [JsonPropertyName("expires_in")]
    public long ExpiresInSeconds { get; set; }

    public TimeSpan ExpiresIn => TimeSpan.FromSeconds(ExpiresInSeconds);
}
