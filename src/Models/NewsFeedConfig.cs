using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetfoundation.Models
{
    public class NewsFeedConfig
    {
        public string OpmlFile { get; set; } = "news.opml"; 
        public string CacheKey { get; set; } = "newsFeed";
        public int CacheDurationInSeconds { get; set; } = 3600; //1 hour
    }
}
