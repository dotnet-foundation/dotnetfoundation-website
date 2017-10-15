using Microsoft.SyndicationFeed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetfoundation.Models
{
    public class NewsItem
    {
        public ISyndicationItem ItemDetails { get; set; }
        public string Source { get; set; }
    }
}
