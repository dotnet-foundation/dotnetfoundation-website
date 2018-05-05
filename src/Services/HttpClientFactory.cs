using System;
using System.Net.Http;

namespace dotnetfoundation.Services
{
    public sealed class HttpClientFactory : IHttpClientFactory
    {
        private readonly Lazy<HttpClient> _lazyHttpClient;

        public HttpClientFactory()
        {
            _lazyHttpClient = new Lazy<HttpClient>(() => new HttpClient(), isThreadSafe: true);
        }

        public HttpClient CreateClient()
        {
            return _lazyHttpClient.Value;
        }
    }
}
