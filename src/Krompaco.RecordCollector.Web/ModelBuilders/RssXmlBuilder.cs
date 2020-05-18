using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Krompaco.RecordCollector.Content.Models;
using Microsoft.AspNetCore.Mvc;

namespace Krompaco.RecordCollector.Web.ModelBuilders
{
    public class RssXmlBuilder
    {
        private readonly Uri siteUrl;

        private readonly Controller controller;

        private readonly List<SinglePage> posts;

        private readonly SyndicationFeed feed;

        public RssXmlBuilder(Uri siteUrl, Controller controller, List<SinglePage> posts, SyndicationFeed feed)
        {
            this.siteUrl = siteUrl;
            this.controller = controller;
            this.posts = posts;
            this.feed = feed;
        }

        public IActionResult BuildActionResult()
        {
            var items = new List<SyndicationItem>();
            var postings = this.posts;

            foreach (var item in postings)
            {
                var postUrl = new Uri(this.siteUrl, item.RelativeUrl);

                var si = new SyndicationItem(
                    item.Title,
                    new TextSyndicationContent(item.Description),
                    postUrl,
                    postUrl.ToString(),
                    item.Date.ToUniversalTime())
                {
                    PublishDate = item.Date.ToUniversalTime(),
                };

                if (item.Categories != null && item.Categories.Any())
                {
                    foreach (var category in item.Categories)
                    {
                        si.Categories.Add(new SyndicationCategory(category));
                    }
                }

                items.Add(si);
            }

            this.feed.Items = items;

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                NewLineOnAttributes = true,
                Indent = true,
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
