using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Krompaco.RecordCollector.Content.Models;
using Tomlyn;
using Tomlyn.Model;

namespace Krompaco.RecordCollector.Content.FrontMatterParsers
{
    public class TomlParser
    {
        private TextReader tr;

        public TomlParser(TextReader tr)
        {
            this.tr = tr;
        }

        public SinglePage GetAsSinglePage()
        {
            var fm = string.Empty;
            var frontMatterOpened = false;

            while (this.tr.Peek() >= 0)
            {
                var line = this.tr.ReadLine()?.Trim();

                if (line == "+++")
                {
                    if (frontMatterOpened)
                    {
                        break;
                    }

                    frontMatterOpened = true;
                    line = this.tr.ReadLine()?.Trim();
                }

                if (frontMatterOpened)
                {
                    fm += line + "\r\n";
                }
            }

            var single = new SinglePage
            {
                CustomArrayProperties = new Dictionary<string, List<string>>(),
                CustomStringProperties = new Dictionary<string, string>(),
                FileResources = new List<FileResource>(),
                PageResources = new List<PageResource>(),
            };

            var doc = Toml.Parse(fm.TrimEnd('\r', '\n'));
            var table = doc.ToModel();

            foreach (var key in table.Keys)
            {
                ////aliases
                ////    an array of one or more aliases (e.g., old published paths of renamed content) that will be created in the output directory structure . See Aliases for details.
                if (key.Equals("aliases", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x.ToString()).ToList();
                    single.Aliases = stringValues.Select(x => new Uri(x, UriKind.Relative)).ToList();
                    continue;
                }

                ////audio
                ////    an array of paths to audio files related to the page; used by the opengraph internal template to populate og:audio.
                if (key.Equals("audio", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x.ToString()).ToList();
                    single.Audio = stringValues.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
                    continue;
                }

                ////cascade
                ////    a map of Front Matter keys whose values are passed down to the page’s descendents unless overwritten by self or a closer ancestor’s cascade. See Front Matter Cascade for details.
                if (key.Equals("cascade", StringComparison.OrdinalIgnoreCase))
                {
                    var cascadeTable = (TomlTable)table["cascade"];

                    foreach (var cascadeKey in cascadeTable.Keys)
                    {
                        if (single.Cascade == null)
                        {
                            single.Cascade = new CascadeVariables
                            {
                                CustomArrayProperties = new Dictionary<string, List<string>>(),
                                CustomStringProperties = new Dictionary<string, string>()
                            };
                        }

                        try
                        {
                            var stringValues = ((TomlArray)cascadeTable[cascadeKey]).Select(x => x.ToString()).ToList();
                            single.Cascade.CustomArrayProperties.Add(cascadeKey, stringValues);
                            continue;
                        }
                        catch (Exception)
                        {
                        }

                        try
                        {
                            var stringValue = cascadeTable[cascadeKey].ToString();
                            single.Cascade.CustomStringProperties.Add(cascadeKey, stringValue);
                        }
                        catch (Exception)
                        {
                        }
                    }

                    continue;
                }

                if (key.Equals("categories", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x.ToString()).ToList();
                    single.Categories = stringValues;
                    continue;
                }

                ////date
                ////    the datetime assigned to this page. This is usually fetched from the date field in front matter, but this behaviour is configurable.
                if (key.Equals("date", StringComparison.OrdinalIgnoreCase))
                {
                    var dateString = (string)table[key];
                    single.Date = DateTime.TryParse(dateString, out var date) ? date.Date : DateTime.MinValue;
                    continue;
                }

                ////description
                ////    the description for the content.
                if (key.Equals("description", StringComparison.OrdinalIgnoreCase))
                {
                    single.Description = (string)table[key];
                    continue;
                }

                ////draft
                ////    if true, the content will not be rendered unless the --buildDrafts flag is passed to the hugo command.
                if (key.Equals("draft", StringComparison.OrdinalIgnoreCase))
                {
                    single.Draft = (bool)table[key];
                    continue;
                }

                ////expiryDate
                ////    the datetime at which the content should no longer be published by Hugo; expired content will not be rendered unless the --buildExpired flag is passed to the hugo command.
                if (key.Equals("expiryDate", StringComparison.OrdinalIgnoreCase))
                {
                    var dateString = (string)table[key];
                    single.Date = DateTime.TryParse(dateString, out var date) ? date.Date : DateTime.MaxValue;
                    continue;
                }

                ////headless
                ////    if true, sets a leaf bundle to be headless.
                if (key.Equals("headless", StringComparison.OrdinalIgnoreCase))
                {
                    single.Headless = (bool)table[key];
                    continue;
                }

                ////images
                ////    an array of paths to images related to the page; used by internal templates such as _internal/twitter_cards.html.
                if (key.Equals("images", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x.ToString()).ToList();
                    single.Images = stringValues.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
                    continue;
                }

                ////isCJKLanguage
                ////    if true, Hugo will explicitly treat the content as a CJK language; both .Summary and .WordCount work properly in CJK languages.
                if (key.Equals("isCJKLanguage", StringComparison.OrdinalIgnoreCase))
                {
                    single.IsCjkLanguage = (bool)table[key];
                    continue;
                }

                ////keywords
                ////    the meta keywords for the content.
                if (key.Equals("keywords", StringComparison.OrdinalIgnoreCase))
                {
                    single.Keywords = (string)table[key];
                    continue;
                }

                ////layout
                ////    the layout Hugo should select from the lookup order when rendering the content. If a type is not specified in the front matter, Hugo will look for the layout of the same name in the layout directory that corresponds with a content’s section. See “Defining a Content Type”
                if (key.Equals("layout", StringComparison.OrdinalIgnoreCase))
                {
                    single.Layout = (string)table[key];
                    continue;
                }

                ////lastmod
                ////    the datetime at which the content was last modified.
                if (key.Equals("lastmod", StringComparison.OrdinalIgnoreCase))
                {
                    var dateString = (string)table[key];
                    single.LastMod = DateTime.TryParse(dateString, out var date) ? date.Date : DateTime.MinValue;
                    continue;
                }

                ////linkTitle
                ////    used for creating links to content; if set, Hugo defaults to using the linktitle before the title. Hugo can also order lists of content by linktitle.
                if (key.Equals("linkTitle", StringComparison.OrdinalIgnoreCase))
                {
                    single.LinkTitle = (string)table[key];
                    continue;
                }

                ////outputs
                ////    allows you to specify output formats specific to the content. See output formats.
                if (key.Equals("outputs", StringComparison.OrdinalIgnoreCase))
                {
                    single.Outputs = (string)table[key];
                    continue;
                }

                ////publishDate
                ////    if in the future, content will not be rendered unless the --buildFuture flag is passed to hugo.
                if (key.Equals("publishDate", StringComparison.OrdinalIgnoreCase))
                {
                    var dateString = (string)table[key];
                    single.PublishDate = DateTime.TryParse(dateString, out var date) ? date.Date : DateTime.MinValue;
                    continue;
                }

                ////resources
                ////    used for configuring page bundle resources. See Page Resources.
                if (key.Equals("resources", StringComparison.OrdinalIgnoreCase))
                {
                    var resourcesTables = ((TomlTableArray)table["resources"]).Select(x => x);

                    foreach (var resourcesTable in resourcesTables)
                    {
                        var fileResource = new FileResource
                        {
                            Params = new Dictionary<string, string>(),
                        };

                        foreach (var resourcesKey in resourcesTable.Keys)
                        {
                            ////Name
                            ////    Default value is the filename (relative to the owning page). Can be set in front matter.
                            if (resourcesKey.Equals("name", StringComparison.OrdinalIgnoreCase))
                            {
                                fileResource.Name = (string)resourcesTable[resourcesKey];
                                continue;
                            }

                            ////Permalink
                            ////    The absolute URL to the resource. Resources of type page will have no value.
                            if (resourcesKey.Equals("src", StringComparison.OrdinalIgnoreCase))
                            {
                                fileResource.Permalink = new Uri((string)resourcesTable[resourcesKey], UriKind.RelativeOrAbsolute);
                                continue;
                            }

                            ////    Title
                            ////    Default value is the same as .Name. Can be set in front matter.
                            if (resourcesKey.Equals("title", StringComparison.OrdinalIgnoreCase))
                            {
                                fileResource.Title = (string)resourcesTable[resourcesKey];
                                continue;
                            }

                            if (!resourcesKey.Equals("params", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            var paramsTable = (TomlTable)resourcesTable[resourcesKey];

                            foreach (var paramsKey in paramsTable.Keys)
                            {
                                var stringValue = paramsTable[paramsKey].ToString();
                                fileResource.Params.Add(paramsKey, stringValue);
                            }
                        }

                        single.FileResources.Add(fileResource);
                    }

                    continue;
                }

                ////series
                ////    an array of series this page belongs to, as a subset of the series taxonomy; used by the opengraph internal template to populate og:see_also.
                if (key.Equals("series", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x.ToString()).ToList();
                    single.Series = stringValues;
                    continue;
                }

                ////slug
                ////    appears as the tail of the output URL. A value specified in front matter will override the segment of the URL based on the filename.
                if (key.Equals("slug", StringComparison.OrdinalIgnoreCase))
                {
                    single.Slug = (string)table[key];
                    continue;
                }

                ////summary
                ////    text used when providing a summary of the article in the .Summary page variable; details available in the content-summaries section.
                if (key.Equals("summary", StringComparison.OrdinalIgnoreCase))
                {
                    single.Summary = (string)table[key];
                    continue;
                }

                if (key.Equals("tags", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x.ToString()).ToList();
                    single.Tags = stringValues;
                    continue;
                }

                ////title
                ////    the title for the content.
                if (key.Equals("title", StringComparison.OrdinalIgnoreCase))
                {
                    single.Title = (string)table[key];
                    continue;
                }

                ////type
                ////    the type of the content; this value will be automatically derived from the directory (i.e., the section) if not specified in front matter.
                if (key.Equals("type", StringComparison.OrdinalIgnoreCase))
                {
                    single.Type = (string)table[key];
                    continue;
                }

                ////url
                ////    the full path to the content from the web root. It makes no assumptions about the path of the content file. It also ignores any language prefixes of the multilingual feature.
                if (key.Equals("url", StringComparison.OrdinalIgnoreCase))
                {
                    var urlString = (string)table[key];
                    single.Url = new Uri(urlString, UriKind.Relative);
                    continue;
                }

                ////videos
                ////    an array of paths to videos related to the page; used by the opengraph internal template to populate og:video.
                if (key.Equals("videos", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x.ToString()).ToList();
                    single.Videos = stringValues.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
                    continue;
                }

                ////weight
                ////    used for ordering your content in lists. Lower weight gets higher precedence. So content with lower weight will come first.
                if (key.Equals("weight", StringComparison.OrdinalIgnoreCase))
                {
                    single.Weight = (int)table[key];
                    continue;
                }

                try
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x.ToString()).ToList();
                    single.CustomArrayProperties.Add(key, stringValues);
                    continue;
                }
                catch (Exception)
                {
                }

                try
                {
                    var stringValue = table[key].ToString();
                    single.CustomStringProperties.Add(key, stringValue);
                }
                catch (Exception)
                {
                }
            }

            return single;
        }
    }
}
