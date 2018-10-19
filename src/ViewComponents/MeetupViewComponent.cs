using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnetfoundation.Services;
using dotnetfoundation.Models;

namespace dotnetfoundation.ViewComponents
{
    public class MeetupViewComponent : ViewComponent
    {
        private readonly MeetupFeedService meetupService;

        public MeetupViewComponent(MeetupFeedService meetupService)
        {
            this.meetupService = meetupService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int quantity, int descriptionLength)
        {
            var feed = await meetupService.GetFeed();
            var items = feed.Events.Take(quantity);

			var model = (items, descriptionLength);
            return View(model);
        }
    }
}
