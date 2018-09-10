using dotnetfoundation.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace dotnetfoundation.Services
{
    public class NewsFeedService
    {
        public NewsFeedService(
            IOptions<NewsFeedConfig> configOptionsAccessor,
            IMemoryCache cache,
            ILogger<NewsFeedService> log
            )
        {
            _config = configOptionsAccessor.Value;
            _cache = cache;
            _log = log;
        }

        private NewsFeedConfig _config;
        private IMemoryCache _cache;
        private ILogger<NewsFeedService> _log;
        private List<NewsItem> _feed = null;

        private async Task<List<NewsItem>> GetFeedInternal()
        {
            var opml = XDocument.Load(_config.OpmlFile);
            var feedData = from item in opml.Descendants("outline")
                           select new
                           {
                               Source = (string)item.Attribute("title"),
                               XmlUrl = (string)item.Attribute("xmlUrl")
                           };

            var feed = new List<NewsItem>();
            foreach (var currentFeed in feedData)
            {
                using (var xmlReader = XmlReader.Create(currentFeed.XmlUrl, new XmlReaderSettings() { Async = true }))
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
                                feed.Add(new NewsItem
                                {
                                    Title = item.Title,
                                    Uri = uri,
                                    Excerpt = item.Description.PlainTextTruncate(120),
                                    PublishDate = item.Published.UtcDateTime,
                                    Source = currentFeed.Source ?? item.Contributors.First().Name ?? item.Contributors.First().Email,
                                    NewsType = GetNewsTypeForUri(uri)
                                });
                            }
                            catch (Exception ex)
                            {
                                _log.LogError(ex.ToString());
                            }
                        }
                    }
                }
            }
            return feed.OrderByDescending(f => f.PublishDate).ToList();
        }

        private string GetNewsTypeForUri(string uri)
        {
            if (uri.Contains("blogs.msdn.microsoft.com")) return "product";
            if (uri.Contains("dotnetfoundation.org")) return "news";
            return "community";
        }

        private async Task<List<NewsItem>> GetOrCreateFeedCacheAsync()
        {
            List<NewsItem> result = null;
            if (!_cache.TryGetValue<List<NewsItem>>(_config.CacheKey, out result))
            {
                result = await GetFeedInternal();

                if (result != null)
                {
                    _cache.Set(
                        _config.CacheKey,
                        result,
                        new MemoryCacheEntryOptions()
                         .SetSlidingExpiration(TimeSpan.FromSeconds(_config.CacheDurationInSeconds))
                         );
                }

            }

            if (result == null) { throw new InvalidOperationException("failed to retrieve news feed"); }

            return result;
        }

        private async Task EnsureFeed()
        {
            if (_feed == null)
            {
                _feed = await GetOrCreateFeedCacheAsync();
            }

        }

        public async Task<List<NewsItem>> GetFeed()
        {
            await EnsureFeed();
            return _feed;
        }
    }
}
