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

        public async Task<ProjectFeed> GetFeed()
        {
            using (var client = new HttpClient())
            {
                var jsonString = await client.GetStringAsync(_config.FeedUrl).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<ProjectFeed>(jsonString);
            }

        }
    }
}
