using System.Globalization;
using System.Text.Json;
using Krompaco.RecordCollector.Content.Models;

namespace Krompaco.RecordCollector.Content.FrontMatterParsers
{
    public class JsonParser<TModel> : ParserBase
        where TModel : SinglePage, new()
    {
        private readonly TextReader tr;

        public JsonParser(TextReader tr, string? fullName)
        {
            this.tr = tr;
            this.FullName = fullName ?? string.Empty;
        }

        public TModel GetAsSinglePage()
        {
            var fm = string.Empty;

            while (this.tr.Peek() >= 0)
            {
                var line = this.tr.ReadLine() ?? string.Empty;

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

            var single = new TModel
            {
                CustomArrayProperties = new Dictionary<string, List<string>>(),
                CustomStringProperties = new Dictionary<string, string>(),
                FileResources = new List<FileResource>(),
                PageResources = new List<PageResource>(),
            };

            using (this.tr)
            {
                single.Content = this.tr.ReadToEnd();
            }

            if (this.FullName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                single.ContentType = ContentType.Html;
            }
            else
            {
                single.ContentType = ContentType.Markdown;
            }

            var options = new JsonDocumentOptions { AllowTrailingCommas = true };

            using var document = JsonDocument.Parse(fm, options);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Name.Equals("aliases", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList();
                    single.Aliases = stringValues.Select(x => new Uri(x, UriKind.Relative)).ToList();
                    continue;
                }

                if (property.Name.Equals("audio", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList();
                    single.Audio = stringValues.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
                    continue;
                }

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
                            var stringValues = cascadeProperty.Value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList();
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
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList();
                    single.Categories = stringValues;
                    continue;
                }

                if (property.Name.Equals("date", StringComparison.OrdinalIgnoreCase))
                {
                    single.Date = DateTime.Parse(property.Value.GetRawText().TrimStart('\"').TrimEnd('\"'), CultureInfo.InvariantCulture);
                    continue;
                }

                if (property.Name.Equals("description", StringComparison.OrdinalIgnoreCase))
                {
                    single.Description = property.Value.GetString();
                    continue;
                }

                if (property.Name.Equals("listcategory", StringComparison.OrdinalIgnoreCase))
                {
                    if (single is ListPage listPage)
                    {
                        listPage.ListCategory = property.Value.GetString();
                    }

                    continue;
                }

                if (property.Name.Equals("draft", StringComparison.OrdinalIgnoreCase))
                {
                    single.Draft = bool.Parse(property.Value.GetRawText().TrimStart('\"').TrimEnd('\"'));
                    continue;
                }

                if (property.Name.Equals("expiryDate", StringComparison.OrdinalIgnoreCase))
                {
                    single.ExpiryDate = property.Value.TryGetDateTime(out var date) ? date.Date : DateTime.MaxValue;
                    continue;
                }

                if (property.Name.Equals("headless", StringComparison.OrdinalIgnoreCase))
                {
                    single.Headless = bool.Parse(property.Value.GetRawText().TrimStart('\"').TrimEnd('\"'));
                    continue;
                }

                if (property.Name.Equals("images", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList();
                    single.Images = stringValues.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
                    continue;
                }

                if (property.Name.Equals("isCJKLanguage", StringComparison.OrdinalIgnoreCase))
                {
                    single.IsCjkLanguage = bool.Parse(property.Value.GetRawText().TrimStart('\"').TrimEnd('\"'));
                    continue;
                }

                if (property.Name.Equals("keywords", StringComparison.OrdinalIgnoreCase))
                {
                    single.Keywords = property.Value.GetString();
                    continue;
                }

                if (property.Name.Equals("layout", StringComparison.OrdinalIgnoreCase))
                {
                    single.Layout = property.Value.GetString();
                    continue;
                }

                if (property.Name.Equals("lastmod", StringComparison.OrdinalIgnoreCase))
                {
                    single.LastMod = property.Value.TryGetDateTime(out var date) ? date.Date : DateTime.MinValue;
                    continue;
                }

                if (property.Name.Equals("linkTitle", StringComparison.OrdinalIgnoreCase))
                {
                    single.LinkTitle = property.Value.GetString();
                    continue;
                }

                if (property.Name.Equals("outputs", StringComparison.OrdinalIgnoreCase))
                {
                    single.Outputs = property.Value.GetString();
                    continue;
                }

                if (property.Name.Equals("publishDate", StringComparison.OrdinalIgnoreCase))
                {
                    single.PublishDate =
                        property.Value.TryGetDateTime(out var date) ? date.Date : DateTime.MinValue;
                    continue;
                }

                if (property.Name.Equals("resources", StringComparison.OrdinalIgnoreCase))
                {
                    var resourcesArray = property.Value.EnumerateArray();

                    foreach (var resource in resourcesArray)
                    {
                        var fileResource = new FileResource { Params = new Dictionary<string, string>(), };

                        foreach (var rp in resource.EnumerateObject())
                        {
                            if (rp.Name.Equals("name", StringComparison.OrdinalIgnoreCase))
                            {
                                fileResource.Name = rp.Value.GetString();
                                continue;
                            }

                            if (rp.Name.Equals("src", StringComparison.OrdinalIgnoreCase))
                            {
                                fileResource.Permalink = new Uri(
                                    rp.Value.GetString() ?? string.Empty,
                                    UriKind.RelativeOrAbsolute);
                                continue;
                            }

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

                if (property.Name.Equals("series", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList();
                    single.Series = stringValues;
                    continue;
                }

                if (property.Name.Equals("slug", StringComparison.OrdinalIgnoreCase))
                {
                    single.Slug = property.Value.GetString();
                    continue;
                }

                if (property.Name.Equals("summary", StringComparison.OrdinalIgnoreCase))
                {
                    single.Summary = property.Value.GetString();
                    continue;
                }

                if (property.Name.Equals("tags", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList();
                    single.Tags = stringValues;
                    continue;
                }

                if (property.Name.Equals("title", StringComparison.OrdinalIgnoreCase))
                {
                    single.Title = property.Value.GetString();
                    continue;
                }

                if (property.Name.Equals("type", StringComparison.OrdinalIgnoreCase))
                {
                    single.Type = property.Value.GetString();
                    continue;
                }

                if (property.Name.Equals("url", StringComparison.OrdinalIgnoreCase))
                {
                    var urlString = property.Value.GetString() ?? string.Empty;
                    single.Url = new Uri(urlString, UriKind.Relative);
                    continue;
                }

                if (property.Name.Equals("videos", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList();
                    single.Videos = stringValues.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
                    continue;
                }

                if (property.Name.Equals("weight", StringComparison.OrdinalIgnoreCase))
                {
                    single.Weight = int.Parse(
                        property.Value.GetRawText().TrimStart('\"').TrimEnd('\"'),
                        NumberStyles.Integer,
                        NumberFormatInfo.CurrentInfo);
                    continue;
                }

                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    var stringValues = property.Value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList();
                    single.CustomArrayProperties.Add(property.Name, stringValues);
                    continue;
                }

                single.CustomStringProperties.Add(property.Name, property.Value.GetRawText().TrimStart('\"').TrimEnd('\"'));
            }

            return single;
        }
    }
}
