using dotnetfoundation.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace dotnetfoundation.Services
{
    public class NewsFeedService
    {
        public NewsFeedService(
            IOptions<NewsFeedConfig> configOptionsAccessor,
            IMemoryCache cache
            )
        {
            _config = configOptionsAccessor.Value;
            _cache = cache;
        }

        private NewsFeedConfig _config;
        private IMemoryCache _cache;
        private NewsFeed _feed = null;

        private async Task<NewsFeed> GetFeedInternal()
        {
            var feed = new NewsFeed();
            foreach (string feedUri in _config.Feeds)
            {
                using (var xmlReader = XmlReader.Create(feedUri, new XmlReaderSettings() { Async = true }))
                {
                    var feedReader = new RssFeedReader(xmlReader);

                    while (await feedReader.Read())
                    {
                        switch (feedReader.ElementType)
                        {
                            // Read category
                            case SyndicationElementType.Category:
                                ISyndicationCategory category = await feedReader.ReadCategory();
                                break;

                            // Read Image
                            case SyndicationElementType.Image:
                                ISyndicationImage image = await feedReader.ReadImage();
                                break;

                            // Read Item
                            case SyndicationElementType.Item:
                                ISyndicationItem item = await feedReader.ReadItem();
                                feed.Items.Add(new NewsItem {
                                    ItemDetails = item,
                                    Source = ".NET Blog" //TODO: Set source from config
                                });
                                break;
                        }
                    }
                }
            }
            return feed;
        }

        private async Task<NewsFeed> GetOrCreateFeedCacheAsync()
        {
            NewsFeed result = null;
            if (!_cache.TryGetValue<NewsFeed>(_config.CacheKey, out result))
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

        public async Task<NewsFeed> GetFeed()
        {
            await EnsureFeed();
            return _feed;
        }

    }
}
