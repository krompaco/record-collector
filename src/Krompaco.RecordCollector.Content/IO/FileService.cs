using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Krompaco.RecordCollector.Content.FrontMatterParsers;
using Krompaco.RecordCollector.Content.Languages;
using Krompaco.RecordCollector.Content.Models;
using Microsoft.Extensions.Logging;

namespace Krompaco.RecordCollector.Content.IO
{
    public class FileService
    {
        private readonly string contentRoot;

        private readonly DirectoryInfo contentRootDirectoryInfo;

        private readonly ContentCultureService contentCultureService;

        private readonly ILogger logger;

        private readonly object allFileModelsLock = new object();

        private string[] allFilesField = null;

        private List<DirectoryInfo> rootDirectoriesField = null;

        private List<CultureInfo> rootCulturesField = null;

        private List<string> sectionsField = null;

        public FileService(string contentRoot, ContentCultureService contentCultureService, ILogger logger)
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
            this.contentRootDirectoryInfo = new DirectoryInfo(this.contentRoot);
            this.contentCultureService = contentCultureService;
            this.logger = logger;
        }

        public bool IsSameDirectory(string path1, string path2)
        {
            path1 = path1.TrimEnd(Path.DirectorySeparatorChar);
            path2 = path2.TrimEnd(Path.DirectorySeparatorChar);

            return path1.Equals(path2, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsSupportedPageFile(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var isSupported = fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                               || fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
            return isSupported;
        }

        public bool IsListPartialPageFile(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var isListPage = fileName.EndsWith("_index.md", StringComparison.OrdinalIgnoreCase)
                              || fileName.EndsWith("_index.html", StringComparison.OrdinalIgnoreCase);
            return isListPage;
        }

        public bool IsIndexPage(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var isIndexPage = fileName.Equals("index.md", StringComparison.OrdinalIgnoreCase)
                             || fileName.Equals("index.html", StringComparison.OrdinalIgnoreCase);
            return isIndexPage;
        }

        public string[] GetAllFileFullNames()
        {
            if (this.allFilesField != null)
            {
                return this.allFilesField;
            }

            var di = new DirectoryInfo(this.contentRoot);
            var files = di.EnumerateFiles("*.*", SearchOption.AllDirectories);
            this.allFilesField = files.Select(x => x.FullName).ToArray();
            return this.allFilesField;
        }

        public List<DirectoryInfo> GetRootDirectories()
        {
            if (this.rootDirectoriesField != null)
            {
                return this.rootDirectoriesField;
            }

            var di = new DirectoryInfo(this.contentRoot);
            var directories = di.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
            this.rootDirectoriesField = directories.ToList();
            return this.rootDirectoriesField;
        }

        public FileInfo GetListPartialPageFileInfo(string directoryFullName)
        {
            var di = new DirectoryInfo(directoryFullName);
            var partials = di.EnumerateFiles("_index.*", SearchOption.TopDirectoryOnly);
            return partials.FirstOrDefault(x => this.IsListPartialPageFile(x.Name));
        }

        public FileInfo GetIndexPageFileInfo(string directoryFullName)
        {
            var di = new DirectoryInfo(directoryFullName);
            var directories = di.EnumerateFiles("index.*", SearchOption.TopDirectoryOnly);
            return directories.FirstOrDefault(x =>
                        this.IsSupportedPageFile(x.Name)
                        && x.Name.StartsWith("index.", StringComparison.OrdinalIgnoreCase));
        }

        public List<CultureInfo> GetRootCultures()
        {
            if (this.rootCulturesField != null)
            {
                return this.rootCulturesField;
            }

            var cultures = new List<CultureInfo>();
            var directories = this.GetRootDirectories();

            foreach (var info in directories)
            {
                if (this.contentCultureService.DoesCultureExist(info.Name))
                {
                    cultures.Add(new CultureInfo(info.Name));
                }
            }

            this.rootCulturesField = cultures;
            return this.rootCulturesField;
        }

        public List<string> GetSections(CultureInfo culture)
        {
            if (this.sectionsField != null)
            {
                return this.sectionsField;
            }

            var sections = new List<string>();

            foreach (var info in this.GetRootDirectories())
            {
                if (this.contentCultureService.DoesCultureExist(info.Name)
                    && culture != null
                    && culture.Name.Equals(info.Name, StringComparison.OrdinalIgnoreCase)
                    && this.GetRootCultures().Any())
                {
                    var directories = info.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
                    return directories.Select(x => x.Name).ToList();
                }

                if (!this.contentCultureService.DoesCultureExist(info.Name))
                {
                    sections.Add(info.Name);
                }
            }

            this.sectionsField = sections;
            return this.sectionsField;
        }

        public string GetSectionFromFullName(string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException(nameof(fullName));
            }

            var allSections = this.GetSections(null);
            var name = fullName.Replace(this.contentRoot, string.Empty);
            var parts = name.Split(Path.DirectorySeparatorChar);

            foreach (var part in parts)
            {
                if (allSections.Contains(part))
                {
                    return part;
                }
            }

            return "/";
        }

        public List<IRecordCollectorFile> GetAllFileModels()
        {
            var allFiles = this.GetAllFileFullNames();
            var rootCultures = this.GetRootCultures();
            var allFileModels = new List<IRecordCollectorFile>();
            this.logger.LogInformation($"Files to process: {allFiles.Length}");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var rootIndexPage = this.GetListPartialPageFileInfo(this.contentRoot);
            ListPage rootPage;

            if (rootIndexPage != null)
            {
                rootPage = this.GetAsFileModel(rootIndexPage.FullName) as ListPage ?? new ListPage();
                rootPage.Level = 1;
                var temp = allFiles.ToList();
                temp.Remove(rootIndexPage.FullName);
                allFiles = temp.ToArray();
            }
            else
            {
                rootPage = new ListPage();
                rootPage.Level = rootCultures.Any() ? 0 : 1;
                rootPage.FullName = string.Empty;
            }

            rootPage.DescendantPages = new List<SinglePage>();
            allFileModels.Add(rootPage);

            var allButRoot = allFiles.Where(x =>
                !x.Equals(
                    rootPage.FullName,
                    StringComparison.OrdinalIgnoreCase));

            Parallel.ForEach(allButRoot, (currentFullName) =>
            {
                if (this.IsListPartialPageFile(currentFullName))
                {
                    if (!(this.GetAsFileModel(currentFullName) is ListPage listPage))
                    {
                        return;
                    }

                    lock (this.allFileModelsLock)
                    {
                        allFileModels.Add(listPage);
                    }
                }
                else if (this.IsSupportedPageFile(currentFullName))
                {
                    var fileModel = this.GetAsFileModel(currentFullName);

                    if (fileModel.GetType() != typeof(SinglePage))
                    {
                        return;
                    }

                    lock (this.allFileModelsLock)
                    {
                        allFileModels.Add(fileModel);
                    }
                }

                if (!(this.GetAsFileModel(currentFullName) is FileResource fileResource))
                {
                    return;
                }

                lock (this.allFileModelsLock)
                {
                    allFileModels.Add(fileResource);
                }
            });

            Parallel.ForEach(allFileModels, (currentModel) =>
                {
                    if (currentModel.GetType() == typeof(SinglePage))
                    {
                        currentModel.Siblings = this.GetSiblings(currentModel, allFileModels);
                    }
                    else
                    {
                        currentModel.Siblings = new List<SinglePage>();
                    }

                    if (currentModel is ListPage listPage)
                    {
                        listPage.DescendantPages = this.GetDescendantSinglePages(currentModel, allFileModels);
                    }

                    currentModel.Parent = this.GetParent(currentModel, allFileModels);
                    currentModel.Descendants = this.GetDescendants(currentModel, allFileModels);
                });

            Parallel.ForEach(allFileModels, (currentModel) =>
            {
                currentModel.Ancestors = this.GetAncestors(currentModel, allFileModels);
            });

            stopwatch.Stop();
            this.logger.LogInformation($"Time to process files: {stopwatch.Elapsed.TotalMilliseconds} ms");

            return allFileModels;
        }

        public IRecordCollectorFile GetAsFileModel(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new NullReferenceException("Don't send null or empty fullName");
            }

            if (this.IsListPartialPageFile(fullName))
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
                list.Section = this.GetSectionFromFullName(fullName);

                return list;
            }

            if (!this.IsListPartialPageFile(fullName)
                && this.IsSupportedPageFile(fullName))
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
                single.Section = this.GetSectionFromFullName(fullName);
                single.IsPage = true;
                single.ClosestSectionDirectory = this.GetNestedSectionDirectory(fullName);

                return single;
            }

            var fileResource = new FileResource();
            fileResource.Name = fileResource.Title = new FileInfo(fullName).Name;
            fileResource.FullName = fullName;
            fileResource.RelativeUrl = this.GetRelativeUrlFromFullName(fullName);
            fileResource.Section = this.GetSectionFromFullName(fullName);

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

            var fileInfo = new FileInfo(fullName);

            if (this.IsIndexPage(fileInfo.Name))
            {
                rootRemoved = Regex.Replace(
                    rootRemoved,
                    @"(index\.md|index\.html)",
                    string.Empty,
                    RegexOptions.IgnoreCase);

                return new Uri("/" + rootRemoved, UriKind.Relative);
            }

            rootRemoved = Regex.Replace(
                rootRemoved,
                @"(\.md|\.html)",
                "/",
                RegexOptions.IgnoreCase);

            if (rootRemoved.Equals("_index/", StringComparison.OrdinalIgnoreCase))
            {
                rootRemoved = Regex.Replace(
                    rootRemoved,
                    @"_index\/",
                    string.Empty,
                    RegexOptions.IgnoreCase);
            }

            return new Uri("/" + rootRemoved, UriKind.Relative);
        }

        public List<SinglePage> GetSiblings(IRecordCollectorFile current, List<IRecordCollectorFile> allFileModels)
        {
            return allFileModels
                .Where(x =>
                    x.GetType() == typeof(SinglePage)
                    && !x.FullName.Equals(
                        current.FullName,
                        StringComparison.OrdinalIgnoreCase)
                    && x.Section == current.Section
                    && x.ClosestSectionDirectory != null
                    && current.ClosestSectionDirectory != null
                    && x.ClosestSectionDirectory.Equals(
                        current.ClosestSectionDirectory,
                        StringComparison.OrdinalIgnoreCase))
                .Select(x => (SinglePage)x)
                .Where(x => !x.Headless)
                .OrderByDescending(x => x.Date)
                .ToList();
        }

        public List<SinglePage> GetDescendantSinglePages(IRecordCollectorFile current, List<IRecordCollectorFile> allFileModels)
        {
            return this.GetDescendants(current, allFileModels)
                .Where(x =>
                    x.GetType() == typeof(SinglePage))
                .Select(x => (SinglePage)x)
                .Where(x => !x.Headless)
                .OrderByDescending(x => x.Date)
                .ToList();
        }

        public List<IRecordCollectorFile> GetDescendants(IRecordCollectorFile current, List<IRecordCollectorFile> allFileModels)
        {
            var fileInfo = new FileInfo(current.FullName);
            var directoryName = fileInfo.DirectoryName;

            return allFileModels
                .Where(x =>
                    ((this.IsListPartialPageFile(current.FullName)
                        && !x.FullName.Equals(current.FullName, StringComparison.OrdinalIgnoreCase)
                        && x.FullName.StartsWith(directoryName, StringComparison.OrdinalIgnoreCase))
                     || (!this.IsListPartialPageFile(current.FullName)
                          && x.FullName.StartsWith(directoryName, StringComparison.OrdinalIgnoreCase)
                          && !this.IsSameDirectory(new FileInfo(x.FullName).DirectoryName, directoryName)))
                    && !this.IsSameDirectory(new FileInfo(x.FullName).DirectoryName, current.FullName))
                .ToList();
        }

        public SinglePage GetParent(IRecordCollectorFile current, List<IRecordCollectorFile> allFileModels)
        {
            if (string.IsNullOrWhiteSpace(current.FullName))
            {
                return null;
            }

            if (current.Level == 1)
            {
                return null;
            }

            var fileInfo = new FileInfo(current.FullName);
            var directoryName = fileInfo.Directory.FullName;

            while (true)
            {
                if (this.IsSameDirectory(this.contentRootDirectoryInfo.FullName, directoryName))
                {
                    return (SinglePage)allFileModels.FirstOrDefault(x => x.Level == 1);
                }

                var directoryInfo = new DirectoryInfo(directoryName);
                var indexPageFileInfo = this.GetListPartialPageFileInfo(directoryInfo.Parent.FullName);

                if (indexPageFileInfo != null)
                {
                    return (SinglePage)allFileModels.FirstOrDefault(x => x.FullName.Equals(indexPageFileInfo.FullName, StringComparison.OrdinalIgnoreCase));
                }

                var supportedFiles = directoryInfo.Parent.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
                    .Where(x => this.IsSupportedPageFile(x.Name))
                    .ToList();

                foreach (var fi in supportedFiles)
                {
                    if (this.IsIndexPage(fi.Name) || this.IsListPartialPageFile(fi.Name))
                    {
                        return (SinglePage)allFileModels.FirstOrDefault(x => x.FullName.Equals(fi.FullName, StringComparison.OrdinalIgnoreCase));
                    }

                    if (supportedFiles.Count == 1)
                    {
                        return (SinglePage)allFileModels.FirstOrDefault(x => x.FullName.Equals(fi.FullName, StringComparison.OrdinalIgnoreCase));
                    }
                }

                if (this.IsSameDirectory(this.contentRootDirectoryInfo.FullName, directoryInfo.Parent.FullName))
                {
                    return (SinglePage)allFileModels.FirstOrDefault(x => x.Level == 1);
                }

                directoryName = directoryInfo.Parent.FullName;
            }
        }

        public List<SinglePage> GetAncestors(IRecordCollectorFile current, List<IRecordCollectorFile> allFileModels)
        {
            var ancestors = new List<SinglePage>();

            while (true)
            {
                if (current.Parent != null)
                {
                    ancestors.Add(current.Parent);
                    current = current.Parent;
                }
                else
                {
                    return ancestors;
                }
            }
        }

        public string GetNestedSectionDirectory(string fullName)
        {
            var fileInfo = new FileInfo(fullName);
            var directoryName = fileInfo.DirectoryName;
            string nestedSection = null;

            while (nestedSection == null)
            {
                var directoryInfo = new DirectoryInfo(directoryName);

                if (this.GetListPartialPageFileInfo(directoryName) != null)
                {
                    nestedSection = directoryInfo.FullName;
                }

                directoryName = directoryInfo.Parent.FullName;

                if (this.IsSameDirectory(this.contentRootDirectoryInfo.FullName, directoryName))
                {
                    nestedSection = this.contentRootDirectoryInfo.FullName;
                }
            }

            return nestedSection;
        }
    }
}
