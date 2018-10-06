using dotnetfoundation.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace dotnetfoundation.Services
{
    public class GenericFeedService
    {
        public GenericFeedService(
            IMemoryCache cache,
            ILogger<GenericFeedService> log,
			IHttpClientFactory httpClientFactory
            )
        {
            _cache = cache;
            _log = log;
			_httpClientFactory = httpClientFactory;
        }

        private IMemoryCache _cache;
        private ILogger<GenericFeedService> _log;
		private IHttpClientFactory _httpClientFactory;
		private List<ISyndicationItem> _feed = null;

        private async Task<List<ISyndicationItem>> GetFeedInternal(string url)
        {
			var http = _httpClientFactory.CreateClient();
			var feed = new List<ISyndicationItem>();
			using(var stream = http.GetStreamAsync(url).Result)
			using (var xmlReader = XmlReader.Create(stream))
			{
				var feedReader = new RssFeedReader(xmlReader);

				while (await feedReader.Read())
				{
					if (feedReader.ElementType == SyndicationElementType.Item)
					{
						ISyndicationItem item = await feedReader.ReadItem();

						if (string.IsNullOrWhiteSpace(item.Description) ||
							!item.Links.First().Uri.IsAbsoluteUri)
						{
							continue;
						}

						var uri = item.Links.First().Uri.AbsoluteUri;
						try
						{
							feed.Add(item);
						}
						catch (Exception ex)
						{
							_log.LogError(ex.ToString());
						}
					}
				}
            }
            return feed.OrderByDescending(f => f.Published).ToList();
        }

        private async Task<List<ISyndicationItem>> GetOrCreateFeedCacheAsync(string url)
        {
            List<ISyndicationItem> result = null;
            if (!_cache.TryGetValue<List<ISyndicationItem>>(url, out result))
            {
                result = await GetFeedInternal(url);

                if (result != null)
                {
                    _cache.Set(
                        url,
                        result,
                        new MemoryCacheEntryOptions()
                         .SetSlidingExpiration(TimeSpan.FromSeconds(3600))
                         );
                }

            }

            if (result == null) { throw new InvalidOperationException("failed to retrieve news feed"); }

            return result;
        }

        private async Task EnsureFeed(string url)
        {
            if (_feed == null)
            {
                _feed = await GetOrCreateFeedCacheAsync(url);
            }
        }

        public async Task<List<ISyndicationItem>> GetFeed(string url)
        {
            await EnsureFeed(url);
            return _feed;
        }
    }
}