using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using cloudscribe.SimpleContent.Models;
using System.Xml.Linq;

namespace DotNetFoundationWebsite.Controllers
{
    public class BlogImportController : Controller
    {
        public BlogImportController(ILogger<HomeController> logger, IBlogService blogService)
        {
            log = logger;
            this.blogService = blogService;
        }

        private ILogger log;
        private IBlogService blogService;

        public async Task<IActionResult> Clear()
        {
            var articles = await blogService.GetPosts(false);
            foreach(var a in articles)
            {
                await blogService.Delete(a.Id);
            }
            return Content($"Clear complete. Current post count (should be zero): {(await blogService.GetPosts(true)).Count.ToString()}");
         }

        public async Task<IActionResult> Index(string feedUrl)
        {
            var articles = new List<Post>();

            using(var client = new HttpClient())
            {
                client.BaseAddress = new Uri(feedUrl);
                var responseMessage = await client.GetAsync(feedUrl);
                var responseString = await responseMessage.Content.ReadAsStringAsync();

                //extract feed items
                XDocument doc = XDocument.Parse(responseString);
                var feedItems = from item in doc.Root.Descendants().First(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item")
                                select new Post
                                {
                                    Content = item.Elements().First(i => i.Name.LocalName == "description").Value,
                                    Slug = CleanSlug(item.Elements().First(i => i.Name.LocalName == "link").Value),
                                    PubDate = ParseDate(item.Elements().First(i => i.Name.LocalName == "pubDate").Value),
                                    Title = item.Elements().First(i => i.Name.LocalName == "title").Value
                                };
                articles = feedItems.ToList();
            }

            foreach(var a in articles)
            {
                await blogService.Create(a);
            }

            return Content($"Import complete. Current post count: {(await blogService.GetPosts(true)).Count.ToString()}");
        }

        private string CleanSlug(string slug)
        {
            int start = slug.LastIndexOf('/');
            return slug.Substring(start + 1);
        }

        private DateTime ParseDate(string date)
        {
            DateTime result;
            if (DateTime.TryParse(date, out result))
                return result;
            else
                return DateTime.MinValue;
        }
    }
}