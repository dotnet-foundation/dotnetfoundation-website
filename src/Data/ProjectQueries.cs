using dotnetfoundation.Models;
using dotnetfoundation.Services;
using cloudscribe.Pagination.Models;
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
        private ProjectRepoFeed _repoFeed = null;
        private ProjectFeed _projectFeed = null;

        private async Task<ProjectRepoFeed> GetOrCreateRepoFeedCacheAsync()
        {
            ProjectRepoFeed result = null;
            if(!_cache.TryGetValue<ProjectRepoFeed>(_config.RepoFeedCacheKey, out result))
            { 
                result = await _feedService.GetRepoFeed();
                if(result != null)
                {
                    _cache.Set(
                        _config.RepoFeedCacheKey,
                        result,
                        new MemoryCacheEntryOptions()
                         .SetSlidingExpiration(TimeSpan.FromSeconds(_config.CacheDurationInSeconds))
                         );
                }

            }

            if(result == null) { throw new InvalidOperationException("failed to retrieve project feed"); }

            return result;
        }

        private async Task<ProjectFeed> GetOrCreateProjectFeedCacheAsync()
        {
            ProjectFeed result = null;
            if (!_cache.TryGetValue<ProjectFeed>(_config.ProjectFeedCacheKey, out result))
            {
                result = await _feedService.GetProjectFeed();
                if (result != null)
                {
                    _cache.Set(
                        _config.ProjectFeedCacheKey,
                        result,
                        new MemoryCacheEntryOptions()
                         .SetSlidingExpiration(TimeSpan.FromSeconds(_config.CacheDurationInSeconds))
                         );
                }

            }

            if (result == null) { throw new InvalidOperationException("failed to retrieve project feed"); }

            return result;
        }

        private async Task EnsureRepoFeed()
        {
            if(_repoFeed == null)
            {
                _repoFeed = await GetOrCreateRepoFeedCacheAsync();
            }
           
        }

        private async Task EnsureProjectFeed()
        {
            if (_projectFeed == null)
            {
                _projectFeed = await GetOrCreateProjectFeedCacheAsync();
            }

        }

        public async Task<ProjectRepoSummary> GetRepoSummary()
        {
            await EnsureRepoFeed();
            return _repoFeed.Summary;
        }

        public async Task<PagedResult<ProjectRepo>> FetchRepos(
            string query,
            int pageNumber = 1,
            int pageSize = 1000,
            CancellationToken cancellationToken = default(CancellationToken)
            )
        {
            await EnsureRepoFeed();
            var result = new PagedResult<ProjectRepo>();

            if(_repoFeed != null && _repoFeed.Projects != null)
            {
                List<ProjectRepo> matches = new List<ProjectRepo>();
                if (string.IsNullOrWhiteSpace(query))
                {
                    matches = _repoFeed.Projects.OrderByDescending(p => p.Awesomeness)
                   .ToList<ProjectRepo>();
                }
                else
                {
                    matches = _repoFeed.Projects.Where(p =>
                     p.Name.Contains(query)
                     || p.Description.Contains(query)
                     || p.Contributor.Contains(query)
                    ).OrderByDescending(p => p.Awesomeness)
                    .ToList<ProjectRepo>();
                }

                var totalItems = matches.Count;

                if (pageSize > 0)
                {
                    var offset = 0;
                    if (pageNumber > 1) { offset = pageSize * (pageNumber - 1); }
                    matches = matches.Skip(offset).Take(pageSize).ToList<ProjectRepo>();

                }
                
                result.Data = matches;
                result.TotalItems = totalItems;
                result.PageNumber = pageNumber;
                result.PageSize = pageSize;
            }
            

            return result;

        }

        public async Task<PagedResult<Project>> FetchProjects(
            string query,
            int pageNumber = 1,
            int pageSize = 1000,
            CancellationToken cancellationToken = default(CancellationToken)
            )
        {
            await EnsureProjectFeed();
            var result = new PagedResult<Project>();

            if (_repoFeed != null && _repoFeed.Projects != null)
            {
                List<Project> matches = new List<Project>();
                if (string.IsNullOrWhiteSpace(query))
                {
                    matches = _projectFeed.Projects.OrderBy(p => p.Name)
                   .ToList<Project>();
                }
                else
                {
                    matches = _projectFeed.Projects.Where(p =>
                     p.Name.Contains(query)
                    // || p.Contributor.Contains(query)
                    ).OrderBy(p => p.Name)
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
                result.PageNumber = pageNumber;
                result.PageSize = pageSize;
            }


            return result;

        }

    }
}
