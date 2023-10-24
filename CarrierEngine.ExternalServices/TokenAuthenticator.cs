using CarrierEngine.ExternalServices.Interfaces;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarrierEngine.ExternalServices;

public class TokenAuthenticator : IOAuth2
{
    private readonly HttpClient _httpClientFactory;

    public enum GrantType
    {

    }

    public TokenAuthenticator(HttpClient httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public async Task<T> Authenticate<T>(Uri url, string grantType, string clientId, string clientSecret, string scope)
    {
        var body = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
        var httpResponseMessage = await _httpClientFactory.PostAsync(url, body);
        var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(responseContent);
    }
}