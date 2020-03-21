using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Krompaco.RecordCollector.Content.Models;

namespace Krompaco.RecordCollector.Content.FrontMatterParsers
{
    public class JsonParser
    {
        private readonly TextReader tr;

        public JsonParser(TextReader tr)
        {
            this.tr = tr;
        }

        public SinglePage GetAsSinglePage()
        {
            var fm = string.Empty;

            while (this.tr.Peek() >= 0)
            {
                var line = this.tr.ReadLine();

                // TODO: Make JSON front matter extraction more robust
                if (line.StartsWith("{", StringComparison.Ordinal))
                {
                    fm += line + "\r\n";
                    continue;
                }

                if (line.StartsWith("}", StringComparison.Ordinal))
                {
                    fm += line + "\r\n";
                    break;
                }

                fm += line + "\r\n";
            }

            var single = new SinglePage
            {
                CustomArrayProperties = new Dictionary<string, List<string>>(),
                CustomStringProperties = new Dictionary<string, string>(),
                FileResources = new List<FileResource>(),
                PageResources = new List<PageResource>(),
            };

            var options = new JsonDocumentOptions { AllowTrailingCommas = true };

            using var document = JsonDocument.Parse(fm, options);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                ////aliases
                ////    an array of one or more aliases (e.g., old published paths of renamed content) that will be created in the output directory structure . See Aliases for details.
                if (property.Name.Equals("aliases", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString()).ToList();
                    single.Aliases = stringValues.Select(x => new Uri(x, UriKind.Relative)).ToList();
                    continue;
                }

                ////audio
                ////    an array of paths to audio files related to the page; used by the opengraph internal template to populate og:audio.
                if (property.Name.Equals("audio", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString()).ToList();
                    single.Audio = stringValues.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
                    continue;
                }

                ////cascade
                ////    a map of Front Matter keys whose values are passed down to the page’s descendents unless overwritten by self or a closer ancestor’s cascade. See Front Matter Cascade for details.
                if (property.Name.Equals("cascade", StringComparison.OrdinalIgnoreCase))
                {
                    var cascadeProperties = property.Value.EnumerateObject();

                    foreach (var cascadeProperty in cascadeProperties)
                    {
                        if (single.Cascade == null)
                        {
                            single.Cascade = new CascadeVariables
                            {
                                CustomArrayProperties = new Dictionary<string, List<string>>(),
                                CustomStringProperties = new Dictionary<string, string>()
                            };
                        }

                        if (cascadeProperty.Value.ValueKind == JsonValueKind.Array)
                        {
                            var stringValues = cascadeProperty.Value.EnumerateArray().Select(x => x.GetString()).ToList();
                            single.Cascade.CustomArrayProperties.Add(cascadeProperty.Name, stringValues);
                            continue;
                        }

                        var stringValue = cascadeProperty.Value.GetRawText().TrimStart('\"').TrimEnd('\"');
                        single.Cascade.CustomStringProperties.Add(cascadeProperty.Name, stringValue);
                    }

                    continue;
                }

                if (property.Name.Equals("categories", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString()).ToList();
                    single.Categories = stringValues;
                    continue;
                }

                ////date
                ////    the datetime assigned to this page. This is usually fetched from the date field in front matter, but this behaviour is configurable.
                if (property.Name.Equals("date", StringComparison.OrdinalIgnoreCase))
                {
                    single.Date = property.Value.TryGetDateTime(out var date) ? date.Date : DateTime.MinValue;
                    continue;
                }

                ////description
                ////    the description for the content.
                if (property.Name.Equals("description", StringComparison.OrdinalIgnoreCase))
                {
                    single.Description = property.Value.GetString();
                    continue;
                }

                ////draft
                ////    if true, the content will not be rendered unless the --buildDrafts flag is passed to the hugo command.
                if (property.Name.Equals("draft", StringComparison.OrdinalIgnoreCase))
                {
                    single.Draft = property.Value.GetBoolean();
                    continue;
                }

                ////expiryDate
                ////    the datetime at which the content should no longer be published by Hugo; expired content will not be rendered unless the --buildExpired flag is passed to the hugo command.
                if (property.Name.Equals("expiryDate", StringComparison.OrdinalIgnoreCase))
                {
                    single.Date = property.Value.TryGetDateTime(out var date) ? date.Date : DateTime.MaxValue;
                    continue;
                }

                ////headless
                ////    if true, sets a leaf bundle to be headless.
                if (property.Name.Equals("headless", StringComparison.OrdinalIgnoreCase))
                {
                    single.Headless = property.Value.GetBoolean();
                    continue;
                }

                ////images
                ////    an array of paths to images related to the page; used by internal templates such as _internal/twitter_cards.html.
                if (property.Name.Equals("images", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString()).ToList();
                    single.Images = stringValues.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
                    continue;
                }

                ////isCJKLanguage
                ////    if true, Hugo will explicitly treat the content as a CJK language; both .Summary and .WordCount work properly in CJK languages.
                if (property.Name.Equals("isCJKLanguage", StringComparison.OrdinalIgnoreCase))
                {
                    single.IsCjkLanguage = property.Value.GetBoolean();
                    continue;
                }

                ////keywords
                ////    the meta keywords for the content.
                if (property.Name.Equals("keywords", StringComparison.OrdinalIgnoreCase))
                {
                    single.Keywords = property.Value.GetString();
                    continue;
                }

                ////layout
                ////    the layout Hugo should select from the lookup order when rendering the content. If a type is not specified in the front matter, Hugo will look for the layout of the same name in the layout directory that corresponds with a content’s section. See “Defining a Content Type”
                if (property.Name.Equals("layout", StringComparison.OrdinalIgnoreCase))
                {
                    single.Layout = property.Value.GetString();
                    continue;
                }

                ////lastmod
                ////    the datetime at which the content was last modified.
                if (property.Name.Equals("lastmod", StringComparison.OrdinalIgnoreCase))
                {
                    single.LastMod = property.Value.TryGetDateTime(out var date) ? date.Date : DateTime.MinValue;
                    continue;
                }

                ////linkTitle
                ////    used for creating links to content; if set, Hugo defaults to using the linktitle before the title. Hugo can also order lists of content by linktitle.
                if (property.Name.Equals("linkTitle", StringComparison.OrdinalIgnoreCase))
                {
                    single.LinkTitle = property.Value.GetString();
                    continue;
                }

                ////outputs
                ////    allows you to specify output formats specific to the content. See output formats.
                if (property.Name.Equals("outputs", StringComparison.OrdinalIgnoreCase))
                {
                    single.Outputs = property.Value.GetString();
                    continue;
                }

                ////publishDate
                ////    if in the future, content will not be rendered unless the --buildFuture flag is passed to hugo.
                if (property.Name.Equals("publishDate", StringComparison.OrdinalIgnoreCase))
                {
                    single.PublishDate =
                        property.Value.TryGetDateTime(out var date) ? date.Date : DateTime.MinValue;
                    continue;
                }

                ////resources
                ////    used for configuring page bundle resources. See Page Resources.
                if (property.Name.Equals("resources", StringComparison.OrdinalIgnoreCase))
                {
                    var resourcesArray = property.Value.EnumerateArray();

                    foreach (var resource in resourcesArray)
                    {
                        var fileResource = new FileResource { Params = new Dictionary<string, string>(), };

                        foreach (var rp in resource.EnumerateObject())
                        {
                            ////Name
                            ////    Default value is the filename (relative to the owning page). Can be set in front matter.
                            if (rp.Name.Equals("name", StringComparison.OrdinalIgnoreCase))
                            {
                                fileResource.Name = rp.Value.GetString();
                                continue;
                            }

                            ////Permalink
                            ////    The absolute URL to the resource. Resources of type page will have no value.
                            if (rp.Name.Equals("src", StringComparison.OrdinalIgnoreCase))
                            {
                                fileResource.Permalink = new Uri(
                                    rp.Value.GetString(),
                                    UriKind.RelativeOrAbsolute);
                                continue;
                            }

                            ////    Title
                            ////    Default value is the same as .Name. Can be set in front matter.
                            if (rp.Name.Equals("title", StringComparison.OrdinalIgnoreCase))
                            {
                                fileResource.Title = rp.Value.GetString();
                                continue;
                            }

                            if (!rp.Name.Equals("params", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            var paramsProperties = rp.Value.EnumerateObject();

                            foreach (var paramsProperty in paramsProperties)
                            {
                                fileResource.Params.Add(paramsProperty.Name, paramsProperty.Value.GetRawText().TrimStart('\"').TrimEnd('\"'));
                            }
                        }

                        single.FileResources.Add(fileResource);
                    }

                    continue;
                }

                ////series
                ////    an array of series this page belongs to, as a subset of the series taxonomy; used by the opengraph internal template to populate og:see_also.
                if (property.Name.Equals("series", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString()).ToList();
                    single.Series = stringValues;
                    continue;
                }

                ////slug
                ////    appears as the tail of the output URL. A value specified in front matter will override the segment of the URL based on the filename.
                if (property.Name.Equals("slug", StringComparison.OrdinalIgnoreCase))
                {
                    single.Slug = property.Value.GetString();
                    continue;
                }

                ////summary
                ////    text used when providing a summary of the article in the .Summary page variable; details available in the content-summaries section.
                if (property.Name.Equals("summary", StringComparison.OrdinalIgnoreCase))
                {
                    single.Summary = property.Value.GetString();
                    continue;
                }

                if (property.Name.Equals("tags", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString()).ToList();
                    single.Tags = stringValues;
                    continue;
                }

                ////title
                ////    the title for the content.
                if (property.Name.Equals("title", StringComparison.OrdinalIgnoreCase))
                {
                    single.Title = property.Value.GetString();
                    continue;
                }

                ////type
                ////    the type of the content; this value will be automatically derived from the directory (i.e., the section) if not specified in front matter.
                if (property.Name.Equals("type", StringComparison.OrdinalIgnoreCase))
                {
                    single.Type = property.Value.GetString();
                    continue;
                }

                ////url
                ////    the full path to the content from the web root. It makes no assumptions about the path of the content file. It also ignores any language prefixes of the multilingual feature.
                if (property.Name.Equals("url", StringComparison.OrdinalIgnoreCase))
                {
                    var urlString = property.Value.GetString();
                    single.Url = new Uri(urlString, UriKind.Relative);
                    continue;
                }

                ////videos
                ////    an array of paths to videos related to the page; used by the opengraph internal template to populate og:video.
                if (property.Name.Equals("videos", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString()).ToList();
                    single.Videos = stringValues.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
                    continue;
                }

                ////weight
                ////    used for ordering your content in lists. Lower weight gets higher precedence. So content with lower weight will come first.
                if (property.Name.Equals("weight", StringComparison.OrdinalIgnoreCase))
                {
                    single.Weight = property.Value.GetInt32();
                    continue;
                }

                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString()).ToList();
                    single.CustomArrayProperties.Add(property.Name, stringValues);
                    continue;
                }

                single.CustomStringProperties.Add(property.Name, property.Value.GetRawText().TrimStart('\"').TrimEnd('\"'));
            }

            return single;
        }
    }
}
