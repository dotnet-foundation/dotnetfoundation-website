using Microsoft.SyndicationFeed;
using System.Collections.Generic;

namespace dotnetfoundation.Models
{
    public class NewsFeed
    {
        public NewsFeed()
        {
            Items = new List<NewsItem>();
        }

        public List<NewsItem> Items { get; set; }
    }
}
