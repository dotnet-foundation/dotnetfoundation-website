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
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace dotnetfoundation.Data
{
    public class AzureSearchProjectQueries : IProjectQueries
    {
        public AzureSearchProjectQueries(JsonProjectQueries projectQueries, SearchIndexClient indexClient)
        {
            _projectQueries = projectQueries;
            _indexClient = indexClient;
        }

        private readonly JsonProjectQueries _projectQueries;
        private readonly SearchIndexClient _indexClient;

        // repos search might be going away, continue to use the old implementation for now
        public Task<ProjectRepoSummary> GetRepoSummary() => _projectQueries.GetRepoSummary();

        public Task<PagedResult<ProjectRepo>> FetchRepos(
            string query, int pageNumber = 1, int pageSize = 1000, CancellationToken cancellationToken = default(CancellationToken)) 
            => _projectQueries.FetchRepos(query, pageNumber, pageSize, cancellationToken);

        public async Task<PagedResult<Project>> FetchProjects(
            string query,
            int pageNumber = 1,
            int pageSize = 1000,
            CancellationToken cancellationToken = default(CancellationToken)
            )
        {
            var searchParameters = new SearchParameters
            {
                Select = new[] { "id", "contributorName", "contributorLogo", "contributorUrl", "descriptionMarkdownFilename", "name" },
                IncludeTotalResultCount = true,
                Skip = (pageNumber - 1) * pageSize,
                Top = pageSize
            };

            var searchResults = await _indexClient.Documents.SearchAsync(query, searchParameters);

            var foundItems = searchResults.Results.Select(r => new Project
            {
                Name = (string)r.Document["descriptionMarkdownFilename"],
                Contributor = new ProjectContributor
                {
                    Name = (string)r.Document["contributorName"],
                    Web = (string)r.Document["contributorUrl"],
                    Logo = (string)r.Document["contributorLogo"]
                }
            })
            .ToList();

            var pagedResult = new PagedResult<Project>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = (int)(searchResults.Count ?? 0),
                Data = foundItems
            };

            return pagedResult;
        }
    }
}
