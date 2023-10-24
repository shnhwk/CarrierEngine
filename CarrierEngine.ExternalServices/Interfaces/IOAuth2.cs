using System;
using System.Threading.Tasks;

namespace CarrierEngine.ExternalServices.Interfaces;

public interface IOAuth2
{
    public Task<T> Authenticate<T>(Uri url, string grantType, string clientId, string clientSecret, string scope);
}