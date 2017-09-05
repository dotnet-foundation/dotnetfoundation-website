using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetfoundation.Models
{
    public class MeetupFeedConfig
    {

        public string FeedFormat { get; set; } = "http://dotnetsocial.cloudapp.net/api/meetup?count={0}&expiry={1}";
        public int NumberToGet { get; set; } = 10;
        public int ExpiryDays { get; set; } = 60;
        public string CacheKey { get; set; } = "meetupFeed";
        public int CacheDurationInSeconds { get; set; } = 18000; //5 hours
    }
}
