using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetfoundation.Models
{
    public class ProjectFeedConfig
    {
        public string RepoFeedUrl { get; set; } = "https://dnfrepos.blob.core.windows.net/output/projects_all.json";
        public string ProjectFeedUrl { get; set; } = "https://raw.githubusercontent.com/dotnet/foundation/master/projects/projects.json";
        public string RepoFeedCacheKey { get; set; } = "projRepoFeed";
        public string ProjectFeedCacheKey { get; set; } = "projFeed";
        public int CacheDurationInSeconds { get; set; } = 36000; //10 hours

    }
}
