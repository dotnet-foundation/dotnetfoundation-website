using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json;
using dotnetfoundation.Services;

namespace DotNetFoundationWebsite.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(
            MeetupFeedService meetupFeedService,
            ILogger<HomeController> logger
            )
        {
            _meetupFeedService = meetupFeedService;
            log = logger;
        }

        private MeetupFeedService _meetupFeedService;
        private ILogger log;

        public IActionResult Index()
        {
            return View();
        }

        [Route("events")]
        public async Task<IActionResult> Events()
        {
            var model = await _meetupFeedService.GetFeed();
            return View(model);
        }

        //[HttpGet]
        //[Route("api/meetup/")]
        //public async Task<IActionResult> Meetups(int count = 10, int expiry = 60)
        //{
        //    dynamic result;
        //    try
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            var url = String.Format("http://dotnetsocial.cloudapp.net/api/meetup?count={0}&expiry={1}", count, expiry);
        //            var response = await client.GetStringAsync(url);
        //            result = JsonConvert.DeserializeObject(response);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        return Content(e.Message);
        //    }
        //    return Ok(result);
        //}
    }
}
