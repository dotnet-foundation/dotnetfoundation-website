using System.Net.Http;

namespace dotnetfoundation.Services
{
    public interface IHttpClientFactory
    {
        HttpClient CreateClient();
    }
}
