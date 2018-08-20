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
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Xml;
using Microsoft.SyndicationFeed.Rss;
using Microsoft.SyndicationFeed;
using System.Threading;
using System.Globalization;

namespace DotNetFoundationWebsite.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class BlogImportController : Controller
    {

        public BlogImportController(
            ILogger<BlogImportController> logger, 
            IBlogService blogService, 
            IProjectSettingsResolver projectResolver,
            IHttpClientFactory httpClientFactory
            )
        {
            _log = logger;
            _blogService = blogService;
            _projectResolver = projectResolver;
            _httpClientFactory = httpClientFactory;
        }

        private readonly ILogger _log;
        private readonly IBlogService _blogService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IProjectSettingsResolver _projectResolver;

        // TODO: Jon this is a dangerous method and should be removed/commented out after import to avoid accidental delete all of posts
        // I added AdminPolicy above so it at least can't be done by an anonymous request
        public async Task<IActionResult> Clear()
        {
            var articles = await _blogService.GetPosts(true);
            foreach (var a in articles)
            {
                await _blogService.Delete(a.Id);
            }
            return Content($"Clear complete. Current post count (should be zero): {(await _blogService.GetPosts(true)).Count.ToString()}");
        }

        public async Task<IActionResult> Index(string feedUrl)
        {
            var articles = new List<Post>();

            var client = _httpClientFactory.CreateClient();
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
                                IsPublished = true,
                                Title = item.Elements().First(i => i.Name.LocalName == "title").Value
                            };
            articles = feedItems.ToList();

            foreach (var a in articles)
            {
                await _blogService.Create(a);
            }

            return Content($"Import complete. Current post count: {(await _blogService.GetPosts(true)).Count.ToString()}");
        }


        public async Task<IActionResult> Export()
        {
            var project = await _projectResolver.GetCurrentProjectSettings(CancellationToken.None);
            var articles = await _blogService.GetPosts(true);

            var baseUrl = string.Concat(HttpContext.Request.Scheme, "://", HttpContext.Request.Host.ToUriComponent());
            var feedUrl = baseUrl + "api/rss";

            var sb = new StringBuilder();

            using (XmlWriter xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings() { Async = true, Indent = true }))
            {
                var writer = new RssFeedWriter(xmlWriter);
                await writer.WriteTitle(project.Title);
                await writer.WriteDescription(".NET Foundation Blog");
                if (!string.IsNullOrEmpty(project.LanguageCode))
                {
                    await writer.WriteLanguage(new CultureInfo(project.LanguageCode));
                }
                
                foreach (var a in articles)
                {
                    var postUrl = await _blogService.ResolvePostUrl(a);

                    var item = new SyndicationItem()
                    {
                        Title = a.Title,
                        Description = a.Content,
                        Id = baseUrl + postUrl,
                        Published = a.PubDate,
                        LastUpdated = a.LastModified
                         
                    };

                    foreach(var c in a.Categories)
                    {
                        item.AddCategory(new SyndicationCategory(c));
                    }

                    item.AddLink(new SyndicationLink(new Uri(baseUrl + postUrl)));

                    
                    //item.AddContributor(new SyndicationPerson("test", "test@mail.com"));

                    await writer.Write(item);
                }

               
                xmlWriter.Flush();
            }

            var result = new ContentResult
            {
                ContentType = "application/xml",
                Content = sb.ToString(),
                StatusCode = 200
            };

            return result;


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