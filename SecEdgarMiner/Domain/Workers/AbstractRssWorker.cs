using Microsoft.Extensions.Logging;
using SecEdgarMiner.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace SecEdgarMiner.Domain.Workers
{
    public abstract class AbstractRssWorker : IRssWorker
    {
        public AbstractRssWorker(IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _client = httpClientFactory.CreateClient("SecEdgarMinerClient");
            _logger = logger;
        }

        protected readonly HttpClient _client;
        protected readonly ILogger _logger;

        private static DateTimeOffset? _feedLastUpdatedTime = DateTimeOffset.UtcNow.AddDays(-1);
        private static string _feedLastETag;

        public async Task<SyndicationFeed> GetRssFeedAsync(string rssUrl, bool latestItemsOnly = true)
        {
            var response = await GetRssServerResponseAsync(rssUrl, latestItemsOnly);

            if (response == null) return null;
            //

            var responseString = await HttpHelper.GetResponseMessageAsync(response);

            if (string.IsNullOrWhiteSpace(responseString))
            {
                _logger.LogInformation($"Rss response has Status: {response.StatusCode} and Content: null");

                return null;
            }

            using (var reader = XmlReader.Create(new StringReader(responseString)))
            {
                var feed = SyndicationFeed.Load(reader);

                _logger.LogInformation($"Feed entries received Count(Total): {feed.Items.Count()}");

                if (latestItemsOnly)
                {
                    feed.Items = feed.Items.Where(item => item.LastUpdatedTime >= _feedLastUpdatedTime).ToList();

                    _logger.LogInformation($"Feed entries received Count(NewOrUpdated): {feed.Items.Count()}");
                }

                _feedLastUpdatedTime = feed.LastUpdatedTime;

                return feed;
            }
        }

        public async Task<IEnumerable<SyndicationItem>> GetRssFeedItemsAsync(string rssUrl, bool latestItemsOnly = true)
        {
            var feed = await GetRssFeedAsync(rssUrl, latestItemsOnly);

            return feed.Items.ToList();
        }

        public virtual Task ProcessRssFeedAsync(SyndicationFeed feed)
        {
            throw new NotImplementedException();
        }

        public virtual Task ProcessRssItemAsync(SyndicationItem rssItem)
        {
            throw new NotImplementedException();
        }

        protected virtual async Task<HttpResponseMessage> GetRssServerResponseAsync(string rssUrl, bool latestItemsOnly)
        {
            _client.DefaultRequestHeaders.Remove("If-Modified-Since");
            if (latestItemsOnly && _feedLastUpdatedTime.HasValue)
            {
                _client.DefaultRequestHeaders.IfModifiedSince = _feedLastUpdatedTime;
            }

            _client.DefaultRequestHeaders.IfNoneMatch.Clear();
            if (latestItemsOnly && !string.IsNullOrWhiteSpace(_feedLastETag))
            {
                _client.DefaultRequestHeaders.IfNoneMatch.TryParseAdd(_feedLastETag);
            }

            var response = await _client.GetAsync(rssUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Rss request failed. Response status is: {response.StatusCode}");
                return null;
            }

            if (response.Headers.TryGetValues("ETag", out IEnumerable<string> eTagHeaders))
            {
                _feedLastETag = eTagHeaders.FirstOrDefault();
            }

            return response;
        }
    }
}
