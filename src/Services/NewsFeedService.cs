using dotnetfoundation.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace dotnetfoundation.Services
{
    public class NewsFeedService
    {
        public NewsFeedService(
            IOptions<NewsFeedConfig> configOptionsAccessor,
            IMemoryCache cache
            )
        {
            _config = configOptionsAccessor.Value;
            _cache = cache;
        }

        private NewsFeedConfig _config;
        private IMemoryCache _cache;
        private NewsFeed _feed = null;

        private async Task<NewsFeed> GetFeedInternal()
        {
            var feed = new NewsFeed();
            foreach (string feedUri in _config.Feeds)
            {
                using (var xmlReader = XmlReader.Create(feedUri, new XmlReaderSettings() { Async = true }))
                {
                    var feedReader = new RssFeedReader(xmlReader);

                    while (await feedReader.Read())
                    {
                        switch (feedReader.ElementType)
                        {
                            // Read category
                            case SyndicationElementType.Category:
                                ISyndicationCategory category = await feedReader.ReadCategory();
                                break;

                            // Read Image
                            case SyndicationElementType.Image:
                                ISyndicationImage image = await feedReader.ReadImage();
                                break;

                            // Read Item
                            case SyndicationElementType.Item:
                                ISyndicationItem item = await feedReader.ReadItem();
                                feed.Items.Add(new NewsItem {
                                    Title = item.Title,
                                    Uri = item.Links.First().Uri.AbsoluteUri,
                                    Excerpt = GetTextDescription(item.Description),
                                    Source = ".NET Blog" //TODO: Set source from config
                                });
                                break;
                        }
                    }
                }
            }
            return feed;
        }

        private async Task<NewsFeed> GetOrCreateFeedCacheAsync()
        {
            NewsFeed result = null;
            if (!_cache.TryGetValue<NewsFeed>(_config.CacheKey, out result))
            {
                result = await GetFeedInternal();
                
                if (result != null)
                {
                    _cache.Set(
                        _config.CacheKey,
                        result,
                        new MemoryCacheEntryOptions()
                         .SetSlidingExpiration(TimeSpan.FromSeconds(_config.CacheDurationInSeconds))
                         );
                }

            }

            if (result == null) { throw new InvalidOperationException("failed to retrieve news feed"); }

            return result;
        }

        private async Task EnsureFeed()
        {
            if (_feed == null)
            {
                _feed = await GetOrCreateFeedCacheAsync();
            }

        }

        public async Task<NewsFeed> GetFeed()
        {
            await EnsureFeed();
            return _feed;
        }

        private static string GetTextDescription(string description)
        {
            string text = HtmlToPlainText(description);
            char[] terminators = { '.', ',', ';', ':', '?', '!' };
            int end = text.LastIndexOfAny(terminators, 120);
            if(end == -1)
            {
                end = text.LastIndexOf(" ", 120);
                return text.Substring(0, end) + "...";
            }
            return text.Substring(0, end + 1);
        }

        //From https://stackoverflow.com/a/16407272/5
        //TODO: Use a proper sanitizer, perhaps https://github.com/atifaziz/High5
        private static string HtmlToPlainText(string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;
            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }
    }
}
