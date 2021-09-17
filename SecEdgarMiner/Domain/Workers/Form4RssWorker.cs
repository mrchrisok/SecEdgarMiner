using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using SecEdgarMiner.Common;
using SecEdgarMiner.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

namespace SecEdgarMiner.Domain.Workers
{
   public class Form4RssWorker : AbstractRssWorker, IForm4RssWorker
   {
	  public Form4RssWorker(IHttpClientFactory httpClientFactory, ILogger<Form4RssWorker> logger)
		 : base(httpClientFactory, logger)
	  {
	  }

	  public async Task<IEnumerable<Form4InfoModel>> GetForm4InfoListAsync(SyndicationFeed feed)
	  {
		 var form4InfoList = new List<Form4InfoModel>();

		 foreach (var rssItem in feed.Items)
		 {
			var form4Info = await GetForm4InfoAsync(rssItem);
			form4InfoList.Add(form4Info);
		 }

		 return form4InfoList.Distinct();
	  }

	  public async Task<Form4InfoModel> GetForm4InfoAsync(SyndicationItem rssItem)
	  {
		 var form4EntryUri = await GetForm4EntryUri(rssItem);
		 var form4XmlDataUri = await GetForm4InfoAsync(form4EntryUri);

		 if (form4XmlDataUri == null)
		 {
			_logger.LogInformation($"Form4XmlDataUrl not resolved from: {rssItem.Links[0].GetAbsoluteUri()}");

			return null;
		 }

		 return form4XmlDataUri;
	  }

	  private Task<Uri> GetForm4EntryUri(SyndicationItem rssItem)
	  {
		 return Task.FromResult(rssItem.Links[0].Uri);
	  }

	  private async Task<Form4InfoModel> GetForm4InfoAsync(Uri form4EntryUri)
	  {
		 var response = await _client.GetAsync(form4EntryUri);
		 var pageString = await HttpHelper.GetResponseMessageAsync(response);

		 var htmlDoc = new HtmlDocument();
		 htmlDoc.LoadHtml(pageString);

		 var form4Info = new Form4InfoModel();
		 var form4DocumentUriLinks = htmlDoc.DocumentNode.Descendants("a")
			.Where(anchor => anchor.GetAttributeValue("href", "").EndsWith(".xml"));

		 var form4HtmlDocumentUriLinks = form4DocumentUriLinks.Where(anchor => anchor.InnerText.EndsWith(".html"));
		 if (form4HtmlDocumentUriLinks.Count() == 1)
		 {
			var form4HtmlDocHref = form4HtmlDocumentUriLinks.First().GetAttributeValue("href", "");
			var form4HtmlUrl = $"https://{form4EntryUri.Host}{form4HtmlDocHref}";
			form4Info.HtmlUrl = form4HtmlUrl;
		 }

		 var form4XmlDocumentUriLinks = form4DocumentUriLinks.Where(anchor => anchor.InnerText.EndsWith(".xml"));
		 if (form4XmlDocumentUriLinks.Count() == 1)
		 {
			var form4XmlDocHref = form4XmlDocumentUriLinks.First().GetAttributeValue("href", "");
			var form4XmlUrl = $"https://{form4EntryUri.Host}{form4XmlDocHref}";
			form4Info.XmlUrl = form4XmlUrl;
		 }

		 return form4Info;
	  }
   }
}
