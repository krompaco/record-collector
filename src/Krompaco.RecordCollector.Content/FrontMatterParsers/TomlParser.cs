using Krompaco.RecordCollector.Content.Models;
using Tomlyn;
using Tomlyn.Model;

namespace Krompaco.RecordCollector.Content.FrontMatterParsers
{
    public class TomlParser<TModel> : ParserBase
        where TModel : SinglePage, new()
    {
        private readonly TextReader tr;

        public TomlParser(TextReader tr, string fullName)
        {
            this.tr = tr;
            this.FullName = fullName;
        }

        public TModel GetAsSinglePage()
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

            var doc = Toml.Parse(fm.TrimEnd('\r', '\n'));
            var table = doc.ToModel();

            foreach (var key in table.Keys)
            {
                if (key.Equals("aliases", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x?.ToString() ?? string.Empty).ToList();
                    single.Aliases = stringValues.Select(x => new Uri(x, UriKind.Relative)).ToList();
                    continue;
                }

                if (key.Equals("audio", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x?.ToString() ?? string.Empty).ToList();
                    single.Audio = stringValues.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
                    continue;
                }

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
                            var stringValues = ((TomlArray)cascadeTable[cascadeKey]).Select(x => x?.ToString() ?? string.Empty).ToList();
                            single.Cascade.CustomArrayProperties.Add(cascadeKey, stringValues);
                            continue;
                        }
                        catch (Exception)
                        {
                        }

                        try
                        {
                            var stringValue = cascadeTable[cascadeKey]?.ToString() ?? string.Empty;
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
                    var stringValues = ((TomlArray)table[key]).Select(x => x?.ToString() ?? string.Empty).ToList();
                    single.Categories = stringValues;
                    continue;
                }

                if (key.Equals("date", StringComparison.OrdinalIgnoreCase))
                {
                    var dateString = (string)table[key];
                    single.Date = DateTime.TryParse(dateString, out var date) ? date.Date : DateTime.MinValue;
                    continue;
                }

                if (key.Equals("description", StringComparison.OrdinalIgnoreCase))
                {
                    single.Description = (string)table[key];
                    continue;
                }

                if (key.Equals("listcategory", StringComparison.OrdinalIgnoreCase))
                {
                    if (single is ListPage listPage)
                    {
                        listPage.ListCategory = (string)table[key];
                    }

                    continue;
                }

                if (key.Equals("draft", StringComparison.OrdinalIgnoreCase))
                {
                    single.Draft = (bool)table[key];
                    continue;
                }

                if (key.Equals("expiryDate", StringComparison.OrdinalIgnoreCase))
                {
                    var dateString = (string)table[key];
                    single.ExpiryDate = DateTime.TryParse(dateString, out var date) ? date.Date : DateTime.MaxValue;
                    continue;
                }

                if (key.Equals("headless", StringComparison.OrdinalIgnoreCase))
                {
                    single.Headless = (bool)table[key];
                    continue;
                }

                if (key.Equals("images", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x?.ToString() ?? string.Empty).ToList();
                    single.Images = stringValues.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
                    continue;
                }

                if (key.Equals("isCJKLanguage", StringComparison.OrdinalIgnoreCase))
                {
                    single.IsCjkLanguage = (bool)table[key];
                    continue;
                }

                if (key.Equals("keywords", StringComparison.OrdinalIgnoreCase))
                {
                    single.Keywords = (string)table[key];
                    continue;
                }

                if (key.Equals("layout", StringComparison.OrdinalIgnoreCase))
                {
                    single.Layout = (string)table[key];
                    continue;
                }

                if (key.Equals("lastmod", StringComparison.OrdinalIgnoreCase))
                {
                    var dateString = (string)table[key];
                    single.LastMod = DateTime.TryParse(dateString, out var date) ? date.Date : DateTime.MinValue;
                    continue;
                }

                if (key.Equals("linkTitle", StringComparison.OrdinalIgnoreCase))
                {
                    single.LinkTitle = (string)table[key];
                    continue;
                }

                if (key.Equals("outputs", StringComparison.OrdinalIgnoreCase))
                {
                    single.Outputs = (string)table[key];
                    continue;
                }

                if (key.Equals("publishDate", StringComparison.OrdinalIgnoreCase))
                {
                    var dateString = (string)table[key];
                    single.PublishDate = DateTime.TryParse(dateString, out var date) ? date.Date : DateTime.MinValue;
                    continue;
                }

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
                            if (resourcesKey.Equals("name", StringComparison.OrdinalIgnoreCase))
                            {
                                fileResource.Name = (string)resourcesTable[resourcesKey];
                                continue;
                            }

                            if (resourcesKey.Equals("src", StringComparison.OrdinalIgnoreCase))
                            {
                                fileResource.Permalink = new Uri((string)resourcesTable[resourcesKey], UriKind.RelativeOrAbsolute);
                                continue;
                            }

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
                                var stringValue = paramsTable[paramsKey]?.ToString() ?? string.Empty;
                                fileResource.Params.Add(paramsKey, stringValue);
                            }
                        }

                        single.FileResources.Add(fileResource);
                    }

                    continue;
                }

                if (key.Equals("series", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x?.ToString() ?? string.Empty).ToList();
                    single.Series = stringValues;
                    continue;
                }

                if (key.Equals("slug", StringComparison.OrdinalIgnoreCase))
                {
                    single.Slug = (string)table[key];
                    continue;
                }

                if (key.Equals("summary", StringComparison.OrdinalIgnoreCase))
                {
                    single.Summary = (string)table[key];
                    continue;
                }

                if (key.Equals("tags", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x?.ToString() ?? string.Empty).ToList();
                    single.Tags = stringValues;
                    continue;
                }

                if (key.Equals("title", StringComparison.OrdinalIgnoreCase))
                {
                    single.Title = (string)table[key];
                    continue;
                }

                if (key.Equals("type", StringComparison.OrdinalIgnoreCase))
                {
                    single.Type = (string)table[key];
                    continue;
                }

                if (key.Equals("url", StringComparison.OrdinalIgnoreCase))
                {
                    var urlString = (string)table[key];
                    single.Url = new Uri(urlString, UriKind.Relative);
                    continue;
                }

                if (key.Equals("videos", StringComparison.OrdinalIgnoreCase))
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x?.ToString() ?? string.Empty).ToList();
                    single.Videos = stringValues.Select(x => new Uri(x, UriKind.RelativeOrAbsolute)).ToList();
                    continue;
                }

                if (key.Equals("weight", StringComparison.OrdinalIgnoreCase))
                {
                    single.Weight = (int)(long)table[key];
                    continue;
                }

                try
                {
                    var stringValues = ((TomlArray)table[key]).Select(x => x?.ToString() ?? string.Empty).ToList();
                    single.CustomArrayProperties.Add(key, stringValues);
                    continue;
                }
                catch (Exception)
                {
                }

                try
                {
                    var stringValue = table[key]?.ToString() ?? string.Empty;
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
