using cloudscribe.Core.Models.Generic;
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

        public async Task<ProjectSummary> GetSummary()
        {
            return await _queries.GetSummary();
        }

        public async Task<PagedResult<Project>> Search(
            string query,
            int pageNumber = 1,
            int pageSize = 1000,
            CancellationToken cancellationToken = default(CancellationToken)
            )
        {
            return await _queries.Fetch(
                query,
                pageNumber,
                pageSize,
                cancellationToken
                );
        }
    }
}
