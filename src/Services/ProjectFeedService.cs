using dotnetfoundation.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace dotnetfoundation.Services
{
    public class ProjectFeedService
    {
        public ProjectFeedService(
            IOptions<ProjectFeedConfig> configOptionsAccessor
            )
        {
            _config = configOptionsAccessor.Value;

        }

        private ProjectFeedConfig _config;

        public async Task<ProjectRepoFeed> GetRepoFeed()
        {
            using (var client = new HttpClient())
            {
                var jsonString = await client.GetStringAsync(_config.RepoFeedUrl).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<ProjectRepoFeed>(jsonString);
            }

        }

        public async Task<ProjectFeed> GetProjectFeed()
        {
            using (var client = new HttpClient())
            {
                var jsonString = await client.GetStringAsync(_config.ProjectFeedUrl).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<ProjectFeed>(jsonString);
            }

        }
    }
}
