using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetfoundation.Models
{
    public class MeetupFeedConfig
    {

        public string FeedFormat { get; set; } = "https://meetupfeeda0cc.blob.core.windows.net/outcontainer/feed.json";
        public string CacheKey { get; set; } = "meetupFeed";
        public int CacheDurationInSeconds { get; set; } = 18000; //5 hours
    }
}
