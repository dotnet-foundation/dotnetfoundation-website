using cloudscribe.Pagination.Models;
using dotnetfoundation.Data;
using dotnetfoundation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace dotnetfoundation.Services
{
    public class ProjectService
    {
        public ProjectService(
            ProjectQueries queries
            )
        {
            _queries = queries;
        }

        private ProjectQueries _queries;

        public async Task<ProjectRepoSummary> GetRepoSummary()
        {
            return await _queries.GetRepoSummary();
        }

        public async Task<PagedResult<ProjectRepo>> SearchRepos(
            string query,
            int pageNumber = 1,
            int pageSize = 1000,
            CancellationToken cancellationToken = default(CancellationToken)
            )
        {
            return await _queries.FetchRepos(
                query,
                pageNumber,
                pageSize,
                cancellationToken
                );
        }

        public async Task<PagedResult<Project>> SearchProjects(
            string query,
            int pageNumber = 1,
            int pageSize = 1000,
            CancellationToken cancellationToken = default(CancellationToken)
            )
        {
            return await _queries.FetchProjects(
                query,
                pageNumber,
                pageSize,
                cancellationToken
                );
        }

        public async Task<List<ProjectContributor>> FetchContributors(
           CancellationToken cancellationToken = default(CancellationToken)
           )
        {
            return await _queries.FetchContributors();
        }
    }
}
