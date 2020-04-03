using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Krompaco.RecordCollector.Content.FrontMatterParsers;
using Krompaco.RecordCollector.Content.Languages;
using Krompaco.RecordCollector.Content.Models;

namespace Krompaco.RecordCollector.Content.IO
{
    public class FileService
    {
        private readonly string contentRoot;

        private readonly ContentCultureService contentCultureService;

        public FileService(string contentRoot, ContentCultureService contentCultureService)
        {
            if (string.IsNullOrWhiteSpace(contentRoot))
            {
                throw new Exception("FileService expects a contentRoot when initialized.");
            }

            if (!Directory.Exists(contentRoot))
            {
                throw new Exception($"Path for contentRoot does not exist: {contentRoot}");
            }

            this.contentRoot = contentRoot;
            this.contentCultureService = contentCultureService;
        }

        public string[] GetAllFileFullNames()
        {
            var di = new DirectoryInfo(this.contentRoot);
            var files = di.EnumerateFiles("*.*", SearchOption.AllDirectories);
            return files.Select(x => x.FullName).ToArray();
        }

        public string[] GetRootDirectories()
        {
            var di = new DirectoryInfo(this.contentRoot);
            var directories = di.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
            return directories.Select(x => x.Name).ToArray();
        }

        public List<CultureInfo> GetRootCultures()
        {
            var cultures = new List<CultureInfo>();
            var directories = this.GetRootDirectories();

            foreach (var dirName in directories)
            {
                if (this.contentCultureService.DoesCultureExist(dirName))
                {
                    cultures.Add(new CultureInfo(dirName));
                }
            }

            return cultures;
        }

        public IFile GetAsFileModel(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new NullReferenceException("Don't send null or empty fullName");
            }

            if (fullName.EndsWith("_index.md", StringComparison.OrdinalIgnoreCase)
                || fullName.EndsWith("_index.html", StringComparison.OrdinalIgnoreCase))
            {
                var sr = new StreamReader(fullName);
                var typeDetector = new TypeDetector(sr);
                ListPage list = null;

                switch (typeDetector.GetFrontMatterType())
                {
                    case FrontMatterType.Json:
                        var jsonParser = new JsonParser<ListPage>(sr, fullName);
                        list = jsonParser.GetAsSinglePage();
                        break;
                    case FrontMatterType.Yaml:
                        var yamlParser = new YamlParser<ListPage>(sr, fullName);
                        list = yamlParser.GetAsSinglePage();
                        break;
                    default:
                        var tomlParser = new TomlParser<ListPage>(sr, fullName);
                        list = tomlParser.GetAsSinglePage();
                        break;
                }

                list.RelativeUrl = this.GetRelativeUrlFromFullName(fullName);
                list.FullName = fullName;
                return list;
            }

            if (fullName.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                || fullName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                var sr = new StreamReader(fullName);

                var summaryExtractor = new SummaryExtractor(sr);
                var summary = summaryExtractor.GetSummaryFromContent();

                var typeDetector = new TypeDetector(sr);
                SinglePage single = null;

                switch (typeDetector.GetFrontMatterType())
                {
                    case FrontMatterType.Json:
                        var jsonParser = new JsonParser<SinglePage>(sr, fullName);
                        single = jsonParser.GetAsSinglePage();
                        break;
                    case FrontMatterType.Yaml:
                        var yamlParser = new YamlParser<SinglePage>(sr, fullName);
                        single = yamlParser.GetAsSinglePage();
                        break;
                    default:
                        var tomlParser = new TomlParser<SinglePage>(sr, fullName);
                        single = tomlParser.GetAsSinglePage();
                        break;
                }

                if (single.Summary == null)
                {
                    single.Summary = summary;
                }

                single.RelativeUrl = this.GetRelativeUrlFromFullName(fullName);
                single.FullName = fullName;

                return single;
            }

            var fileResource = new FileResource();
            fileResource.Name = fileResource.Title = new FileInfo(fullName).Name;
            fileResource.FullName = fullName;
            fileResource.RelativeUrl = this.GetRelativeUrlFromFullName(fullName);
            return fileResource;
        }

        public Uri GetRelativeUrlFromFullName(string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException(nameof(fullName));
            }

            var rootRemoved = fullName.Replace(this.contentRoot, string.Empty);
            rootRemoved = rootRemoved.TrimStart('/', '\\');

            if (rootRemoved.Contains('\\'))
            {
                rootRemoved = rootRemoved.Replace('\\', '/');
            }

            rootRemoved = Regex.Replace(
                rootRemoved,
                @"(\.md|\.html)",
                "/",
                RegexOptions.IgnoreCase);

            return new Uri("/" + rootRemoved, UriKind.Relative);
        }
    }
}
