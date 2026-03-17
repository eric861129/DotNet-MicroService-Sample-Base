using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Enterprise.Security;

public sealed class ClientCredentialsServiceTokenProvider(
    HttpClient httpClient,
    IOptions<InternalClientCredentialsOptions> options)
    : IServiceTokenProvider
{
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var value = options.Value;

        // 內部服務互相呼叫前，先向 Auth Service 換一張 token。
        // 這樣每個服務都能知道「來的人是誰」。
        using var request = new HttpRequestMessage(HttpMethod.Post, value.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = value.ClientId,
                ["client_secret"] = value.ClientSecret,
                ["scope"] = value.Scope
            }!)
        };

        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("內部 Auth Service 回傳的 token payload 為空。");

        return payload.AccessToken;
    }

    private sealed record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
}
