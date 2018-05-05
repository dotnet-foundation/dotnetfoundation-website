using dotnetfoundation.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace dotnetfoundation.Services
{
    public class MeetupFeedService
    {
        public MeetupFeedService(
            IOptions<MeetupFeedConfig> configOptionsAccessor,
            IMemoryCache cache,
            IHttpClientFactory httpClientFactory
            )
        {
            _config = configOptionsAccessor.Value;
            _cache = cache;
            _httpClientFactory = httpClientFactory;
        }

        private MeetupFeedConfig _config;
        private IMemoryCache _cache;
        private MeetupFeed _feed = null;
        private readonly IHttpClientFactory _httpClientFactory;

        private async Task<MeetupFeed> GetFeedInternal()
        {
            var url = string.Format(_config.FeedFormat, _config.NumberToGet, _config.ExpiryDays);
            var http = _httpClientFactory.CreateClient();
            var jsonString = await http.GetStringAsync(url).ConfigureAwait(false);
            var list = JsonConvert.DeserializeObject<List<MeetupEvent>>(jsonString);
            var feed = new MeetupFeed();
            feed.Events = list;
            return feed;
        }

        private async Task<MeetupFeed> GetOrCreateFeedCacheAsync()
        {
            MeetupFeed result = null;
            if (!_cache.TryGetValue<MeetupFeed>(_config.CacheKey, out result))
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

            if (result == null) { throw new InvalidOperationException("failed to retrieve meetup feed"); }


            return result;
        }

        private async Task EnsureFeed()
        {
            if (_feed == null)
            {
                _feed = await GetOrCreateFeedCacheAsync();
            }

        }

        public async Task<MeetupFeed> GetFeed()
        {
            await EnsureFeed();
            return _feed;
        }

    }
}
