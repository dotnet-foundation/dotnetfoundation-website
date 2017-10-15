using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetfoundation.Models
{
    public class NewsFeedConfig
    {
        public string[] Feeds { get; set; } = new string[] { "https://blogs.msdn.microsoft.com/dotnet/feed/" }; 
        public string CacheKey { get; set; } = "newsFeed";
        public int CacheDurationInSeconds { get; set; } = 3600; //1 hour
    }
}
