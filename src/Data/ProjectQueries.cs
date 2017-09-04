using dotnetfoundation.Models;
using dotnetfoundation.Services;
using cloudscribe.Core.Models.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace dotnetfoundation.Data
{
    public class ProjectQueries
    {
        public ProjectQueries(
            IOptions<ProjectFeedConfig> configOptionsAccessor,
            ProjectFeedService feedService,
            IMemoryCache cache
            )
        {
            _config = configOptionsAccessor.Value;
            _feedService = feedService;
            _cache = cache;
        }

        private ProjectFeedService _feedService;
        private IMemoryCache _cache;
        private ProjectFeedConfig _config;
        private ProjectFeed _feed = null;

        private async Task<ProjectFeed> GetOrCreateFeedCacheAsync()
        {
            ProjectFeed result = null;
            if(!_cache.TryGetValue<ProjectFeed>(_config.CacheKey, out result))
            { 
                result = await _feedService.GetFeed();
                if(result != null)
                {
                    _cache.Set(
                        _config.CacheKey,
                        result,
                        new MemoryCacheEntryOptions()
                         .SetSlidingExpiration(TimeSpan.FromSeconds(_config.CacheDurationInSeconds))
                         );
                }

            }

            if(result == null) { throw new InvalidOperationException("failed to retrieve project feed"); }


            return result;
        }

        private async Task EnsureFeed()
        {
            if(_feed == null)
            {
                _feed = await GetOrCreateFeedCacheAsync();
            }
           
        }

        public async Task<ProjectSummary> GetSummary()
        {
            await EnsureFeed();
            return _feed.Summary;
        }

        public async Task<PagedResult<Project>> Fetch(
            string query,
            int pageNumber = 1,
            int pageSize = 1000,
            CancellationToken cancellationToken = default(CancellationToken)
            )
        {
            await EnsureFeed();
            var result = new PagedResult<Project>();

            if(_feed != null && _feed.Projects != null)
            {
                List<Project> matches = new List<Project>();
                if (string.IsNullOrWhiteSpace(query))
                {
                    matches = _feed.Projects.OrderByDescending(p => p.Awesomeness)
                   .ToList<Project>();
                }
                else
                {
                    matches = _feed.Projects.Where(p =>
                     p.Name.Contains(query)
                     || p.Description.Contains(query)
                     || p.Contributor.Contains(query)
                    ).OrderByDescending(p => p.Awesomeness)
                    .ToList<Project>();
                }

                var totalItems = matches.Count;

                if (pageSize > 0)
                {
                    var offset = 0;
                    if (pageNumber > 1) { offset = pageSize * (pageNumber - 1); }
                    matches = matches.Skip(offset).Take(pageSize).ToList<Project>();

                }
                
                result.Data = matches;
                result.TotalItems = totalItems;
            }
            

            return result;

        }

    }
}
