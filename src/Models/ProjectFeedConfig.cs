using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetfoundation.Models
{
    public class ProjectFeedConfig
    {
        public string FeedUrl { get; set; } = "https://dnfrepos.blob.core.windows.net/output/projects_all.json";
        public string CacheKey { get; set; } = "projectFeed";
        public int CacheDurationInSeconds { get; set; } = 36000; //10 hours

    }
}
