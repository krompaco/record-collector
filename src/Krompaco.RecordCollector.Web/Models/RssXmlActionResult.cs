using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Krompaco.RecordCollector.Content.Models;
using Microsoft.AspNetCore.Mvc;

namespace Krompaco.RecordCollector.Web.Models
{
    public class RssXmlActionResult
    {
        private readonly Uri siteUrl;

        private readonly Controller controller;

        private readonly List<SinglePage> posts;

        private readonly SyndicationFeed feed;

        public RssXmlActionResult(Uri siteUrl, Controller controller, List<SinglePage> posts, SyndicationFeed feed)
        {
            this.siteUrl = siteUrl;
            this.controller = controller;
            this.posts = posts;
            this.feed = feed;
        }

        public IActionResult Get()
        {
            var items = new List<SyndicationItem>();
            var postings = this.posts;

            foreach (var item in postings)
            {
                var postUrl = new Uri(this.siteUrl, item.RelativeUrl);
                items.Add(
                    new SyndicationItem(
                        item.Title,
                        new TextSyndicationContent(item.Description),
                        postUrl,
                        postUrl.ToString(),
                        item.Date.ToUniversalTime()));
            }

            this.feed.Items = items;

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                NewLineOnAttributes = true,
                Indent = true
            };

            using var stream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(stream, settings))
            {
                var rssFormatter = new Rss20FeedFormatter(this.feed, false);
                rssFormatter.WriteTo(xmlWriter);
                xmlWriter.Flush();
            }

            return this.controller.File(stream.ToArray(), "application/rss+xml; charset=utf-8");
        }
    }
}
