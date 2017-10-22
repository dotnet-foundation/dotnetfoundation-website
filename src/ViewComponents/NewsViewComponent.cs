using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnetfoundation.Services;
using dotnetfoundation.Models;

namespace dotnetfoundation.ViewComponents
{
    public class NewsViewComponent : ViewComponent
    {
        private readonly NewsFeedService newsService;

        public NewsViewComponent(NewsFeedService newsService)
        {
            this.newsService = newsService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int quantity)
        {
            var feed = await newsService.GetFeed();
            var items = feed.Items.ToList().Take(quantity);

            return View(items);
        }
    }
}
