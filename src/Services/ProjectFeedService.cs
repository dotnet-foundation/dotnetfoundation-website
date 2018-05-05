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
            IOptions<ProjectFeedConfig> configOptionsAccessor,
            IHttpClientFactory httpClientFactory
            )
        {
            _config = configOptionsAccessor.Value;
            _httpClientFactory = httpClientFactory;
        }

        private ProjectFeedConfig _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public async Task<ProjectRepoFeed> GetRepoFeed()
        {
            var client = _httpClientFactory.CreateClient();
            var jsonString = await client.GetStringAsync(_config.RepoFeedUrl).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<ProjectRepoFeed>(jsonString);
        }

        public async Task<ProjectFeed> GetProjectFeed()
        {
            var client = _httpClientFactory.CreateClient();
            var jsonString = await client.GetStringAsync(_config.ProjectFeedUrl).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<ProjectFeed>(jsonString);
        }
    }
}
