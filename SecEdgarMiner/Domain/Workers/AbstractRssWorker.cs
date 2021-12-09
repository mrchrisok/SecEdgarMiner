using Microsoft.Extensions.Logging;
using SecEdgarMiner.Common;
using SecEdgarMiner.Contracts;
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
        protected readonly IRssFeedArgs _args;

        public virtual async Task<SyndicationFeed> GetRssFeedAsync(string rssUrl, IRssFeedArgs args)
        {
            var response = await GetRssServerResponseAsync(rssUrl, args);

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

                if (args.LatestItemsOnly)
                {
                    feed.Items = feed.Items.Where(item => item.LastUpdatedTime >= args.State.LastUpdatedTime).ToList();

                    _logger.LogInformation($"Feed entries received Count(NewOrUpdated): {feed.Items.Count()}");
                }

                args.State.LastUpdatedTime = feed.LastUpdatedTime;

                return feed;
            }
        }

        public virtual async Task<IEnumerable<SyndicationItem>> GetRssFeedItemsAsync(string rssUrl, IRssFeedArgs args)
        {
            var feed = await GetRssFeedAsync(rssUrl, args);

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

        private async Task<HttpResponseMessage> GetRssServerResponseAsync(string rssUrl, IRssFeedArgs args)
        {
            _client.DefaultRequestHeaders.Remove("If-Modified-Since");
            if (args.LatestItemsOnly)
            {
                _client.DefaultRequestHeaders.IfModifiedSince = args.State.LastUpdatedTime;
            }

            _client.DefaultRequestHeaders.IfNoneMatch.Clear();
            if (args.LatestItemsOnly && !string.IsNullOrWhiteSpace(args.State.LastETag))
            {
                _client.DefaultRequestHeaders.IfNoneMatch.TryParseAdd(args.State.LastETag);
            }

            var response = await _client.GetAsync(rssUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Rss request failed. Response status is: {response.StatusCode}");
                return null;
            }

            if (response.Headers.TryGetValues("ETag", out IEnumerable<string> eTagHeaders))
            {
                args.State.LastETag = eTagHeaders.FirstOrDefault();
            }

            return response;
        }
    }
}
