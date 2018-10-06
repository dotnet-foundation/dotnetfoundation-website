using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnetfoundation.Services;
using dotnetfoundation.Models;

namespace dotnetfoundation.ViewComponents
{
    public class FeedViewComponent : ViewComponent
    {
        private readonly GenericFeedService feedService;

        public FeedViewComponent(GenericFeedService feedService)
        {
            this.feedService = feedService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string url, int quantity)
        {
            var feed = await feedService.GetFeed(url);
            var items = feed.Take(quantity);

            return View(items);
        }
    }
}
