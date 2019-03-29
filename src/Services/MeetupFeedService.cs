using dotnetfoundation.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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
			IHttpClientFactory httpClientFactory,
			ILogger<MeetupFeedService> log
			)
		{
			_config = configOptionsAccessor.Value;
			_cache = cache;
			_httpClientFactory = httpClientFactory;
			_log = log;
		}

		private MeetupFeedConfig _config;
		private IMemoryCache _cache;
		private MeetupFeed _feed = null;
		private readonly IHttpClientFactory _httpClientFactory;
		private ILogger<MeetupFeedService> _log;

		private async Task<MeetupFeed> GetFeedInternal()
		{
			var url = string.Format(_config.FeedFormat);
			var http = _httpClientFactory.CreateClient();
			var feed = new MeetupFeed();

			try
			{
				var jsonString = await http.GetStringAsync(url).ConfigureAwait(false);
				var data = JsonConvert.DeserializeObject<MeetupEventData>(jsonString);
				if (data != null)
				{
					feed.Events = data.Results;
				}
			}
			catch (Exception ex)
			{
				_log.LogError(ex.ToString());
			}
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
						TimeSpan.FromSeconds(_config.CacheDurationInSeconds));
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
