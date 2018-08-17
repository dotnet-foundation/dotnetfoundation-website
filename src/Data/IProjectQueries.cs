using cloudscribe.Pagination.Models;
using dotnetfoundation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace dotnetfoundation.Data
{
    public interface IProjectQueries
    {
        Task<ProjectRepoSummary> GetRepoSummary();
        Task<PagedResult<ProjectRepo>> FetchRepos(string query, int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<PagedResult<Project>> FetchProjects(string query, int pageNumber, int pageSize, CancellationToken cancellationToken);
    }
}
