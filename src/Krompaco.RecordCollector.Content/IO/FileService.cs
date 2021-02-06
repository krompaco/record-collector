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

        private readonly string[] sectionsToExcludeFromLists;

        private readonly object allFileModelsLock = new object();

        private string[] allFilesField = null;

        private List<DirectoryInfo> rootDirectoriesField = null;

        private List<CultureInfo> rootCulturesField = null;

        private List<string> sectionsField = null;

        public FileService(string contentRoot, string[] sectionsToExcludeFromLists, ContentCultureService contentCultureService, ILogger logger)
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
            this.sectionsToExcludeFromLists = sectionsToExcludeFromLists;
        }

        public static bool IsSameDirectory(string path1, string path2)
        {
            if (path1 == null)
            {
                throw new ArgumentNullException(nameof(path1));
            }

            if (path2 == null)
            {
                throw new ArgumentNullException(nameof(path2));
            }

            path1 = path1.TrimEnd(Path.DirectorySeparatorChar);
            path2 = path2.TrimEnd(Path.DirectorySeparatorChar);

            return path1.Equals(path2, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsSupportedPageFile(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var isSupported = fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                               || fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
            return isSupported;
        }

        public static bool IsListPartialPageFile(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var isListPage = fileName.EndsWith("_index.md", StringComparison.OrdinalIgnoreCase)
                              || fileName.EndsWith("_index.html", StringComparison.OrdinalIgnoreCase);
            return isListPage;
        }

        public static bool IsIndexPage(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var isIndexPage = fileName.Equals("index.md", StringComparison.OrdinalIgnoreCase)
                             || fileName.Equals("index.html", StringComparison.OrdinalIgnoreCase);
            return isIndexPage;
        }

        public static FileInfo GetListPartialPageFileInfo(string directoryFullName)
        {
            var di = new DirectoryInfo(directoryFullName);
            var partials = di.EnumerateFiles("_index.*", SearchOption.TopDirectoryOnly);
            return partials.FirstOrDefault(x => IsListPartialPageFile(x.Name));
        }

        public static FileInfo GetIndexPageFileInfo(string directoryFullName)
        {
            var di = new DirectoryInfo(directoryFullName);
            var directories = di.EnumerateFiles("index.*", SearchOption.TopDirectoryOnly);
            return directories.FirstOrDefault(x =>
                IsSupportedPageFile(x.Name)
                && x.Name.StartsWith("index.", StringComparison.OrdinalIgnoreCase));
        }

        public static List<SinglePage> GetSiblingsAndSetNextAndPrevious(IRecordCollectorFile current, List<IRecordCollectorFile> allFileModels)
        {
            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }

            var allOnSameLevel = allFileModels
                .Where(x =>
                    x.GetType() == typeof(SinglePage)
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

            var i = 0;

            foreach (var sp in allOnSameLevel)
            {
                if (sp.FullName.Equals(
                    current.FullName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    if (i > 0 && allOnSameLevel.Count > i - 1)
                    {
                        var previous = allOnSameLevel[i - 1];
                        current.PreviousPage = previous;
                    }

                    if (allOnSameLevel.Count > i + 1 && allOnSameLevel.Count > 1)
                    {
                        var next = allOnSameLevel[i + 1];
                        current.NextPage = next;
                        break;
                    }
                }

                i++;
            }

            return allOnSameLevel
                .Where(x => !x.FullName.Equals(
                    current.FullName,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static List<SinglePage> GetAncestors(IRecordCollectorFile current)
        {
            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }

            var ancestors = new List<SinglePage>();
            var original = current;
            var input = current;

            while (true)
            {
                if (input.ParentPage != null)
                {
                    ancestors.Add(input.ParentPage);
                    input = input.ParentPage;
                }
                else
                {
                    if (original.Level == -1 && ancestors.Count > 0)
                    {
                        original.Level = ancestors.Count;
                    }

                    return ancestors;
                }
            }
        }

        // Non static
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

        public List<string> GetSections()
        {
            if (this.sectionsField != null)
            {
                return this.sectionsField;
            }

            var sections = new List<string>();

            foreach (var info in this.GetRootDirectories())
            {
                if (this.contentCultureService.DoesCultureExist(info.Name)
                    && this.GetRootCultures().Any())
                {
                    var directories = info.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
                    sections.AddRange(directories.Select(x => x.Name));
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

            var allSections = this.GetSections();
            var name = fullName.Replace(this.contentRoot, string.Empty, StringComparison.OrdinalIgnoreCase);
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

            var rootIndexPage = GetListPartialPageFileInfo(this.contentRoot);
            ListPage rootPage;

            if (rootIndexPage != null)
            {
                rootPage = this.GetAsFileModel(rootIndexPage.FullName) as ListPage ?? new ListPage();
                rootPage.Level = rootCultures.Any() ? 0 : 1;
                var temp = allFiles.ToList();
                temp.Remove(rootIndexPage.FullName);
                allFiles = temp.ToArray();
                rootPage.RelativeUrl = new Uri("/", UriKind.Relative);
            }
            else
            {
                rootPage = new ListPage();
                rootPage.Level = rootCultures.Any() ? 0 : 1;
                rootPage.FullName = string.Empty;
                rootPage.IsVirtual = true;
                rootPage.RelativeUrl = new Uri("/", UriKind.Relative);
            }

            rootPage.DescendantPages = new List<SinglePage>();
            allFileModels.Add(rootPage);

            foreach (var culture in rootCultures)
            {
                var cultureRootPath = Path.Combine(this.contentRoot, culture.Name);

                var cultureRootIndexPage = GetListPartialPageFileInfo(cultureRootPath);
                ListPage cultureRootPage;

                if (cultureRootIndexPage != null)
                {
                    cultureRootPage = this.GetAsFileModel(cultureRootIndexPage.FullName) as ListPage ?? new ListPage();
                    cultureRootPage.Level = 1;
                    var temp = allFiles.ToList();
                    temp.Remove(cultureRootPage.FullName);
                    allFiles = temp.ToArray();
                }
                else
                {
                    cultureRootPage = new ListPage();
                    cultureRootPage.Level = 1;
                    cultureRootPage.FullName = string.Empty;
                    cultureRootPage.Title = culture.DisplayName;
                    cultureRootPage.IsVirtual = true;
                }

                cultureRootPage.Culture = culture;
                cultureRootPage.RelativeUrl = new Uri("/" + culture.Name + "/", UriKind.Relative);
                cultureRootPage.DescendantPages = new List<SinglePage>();
                allFileModels.Add(cultureRootPage);
            }

            Parallel.ForEach(allFiles, (currentFullName) =>
            {
                if (IsListPartialPageFile(currentFullName))
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
                else if (IsSupportedPageFile(currentFullName))
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
                        currentModel.Siblings = GetSiblingsAndSetNextAndPrevious(currentModel, allFileModels);
                    }
                    else
                    {
                        currentModel.Siblings = new List<SinglePage>();
                    }

                    if (currentModel is ListPage listPage)
                    {
                        listPage.DescendantPages = this.GetDescendantSinglePages(currentModel, allFileModels);
                    }

                    currentModel.ParentPage = this.GetParent(currentModel, allFileModels);
                    currentModel.Descendants = this.GetDescendants(currentModel, allFileModels);
                });

            Parallel.ForEach(allFileModels, (currentModel) =>
            {
                currentModel.Ancestors = GetAncestors(currentModel);
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

            if (IsListPartialPageFile(fullName))
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

            if (!IsListPartialPageFile(fullName)
                && IsSupportedPageFile(fullName))
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

            var rootRemoved = fullName.Replace(this.contentRoot, string.Empty, StringComparison.OrdinalIgnoreCase);
            rootRemoved = rootRemoved.TrimStart('/', '\\');

            if (rootRemoved.Contains('\\', StringComparison.Ordinal))
            {
                rootRemoved = rootRemoved.Replace('\\', '/');
            }

            var fileInfo = new FileInfo(fullName);

            if (IsIndexPage(fileInfo.Name))
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

            if (rootRemoved.EndsWith("_index/", StringComparison.OrdinalIgnoreCase))
            {
                rootRemoved = Regex.Replace(
                    rootRemoved,
                    @"_index\/",
                    string.Empty,
                    RegexOptions.IgnoreCase);
            }

            return new Uri("/" + rootRemoved, UriKind.Relative);
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
            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }

            if (string.IsNullOrWhiteSpace(current.FullName))
            {
                var rootWithCulturePath = this.GetRootCultures().Any() && current.Culture != null ? Path.Combine(this.contentRoot, current.Culture.Name) : this.contentRoot;

                return allFileModels
                    .Where(x =>
                        x.FullName.StartsWith(rootWithCulturePath, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var fileInfo = new FileInfo(current.FullName);
            var directoryName = fileInfo.DirectoryName;

            return allFileModels
                .Where(x =>
                    ((IsListPartialPageFile(current.FullName)
                        && !x.FullName.Equals(current.FullName, StringComparison.OrdinalIgnoreCase)

                        // ReSharper disable once AssignNullToNotNullAttribute
                        && x.FullName.StartsWith(directoryName, StringComparison.OrdinalIgnoreCase)
                        && this.sectionsToExcludeFromLists != null
                        && !this.sectionsToExcludeFromLists.Any(y => y.Equals(x.Section, StringComparison.OrdinalIgnoreCase)))
                     || (!IsListPartialPageFile(current.FullName)

                         // ReSharper disable once AssignNullToNotNullAttribute
                         && x.FullName.StartsWith(directoryName, StringComparison.OrdinalIgnoreCase)
                          && !IsSameDirectory(new FileInfo(x.FullName).DirectoryName, directoryName)))
                    && !IsSameDirectory(new FileInfo(x.FullName).DirectoryName, current.FullName))
                .ToList();
        }

        public SinglePage GetParent(IRecordCollectorFile current, List<IRecordCollectorFile> allFileModels)
        {
            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }

            if (string.IsNullOrWhiteSpace(current.FullName))
            {
                return null;
            }

            if (current.Level == 1)
            {
                return null;
            }

            var fileInfo = new FileInfo(current.FullName);

            // ReSharper disable once PossibleNullReferenceException
            var directoryName = fileInfo.Directory.FullName;

            while (true)
            {
                if (IsSameDirectory(this.contentRootDirectoryInfo.FullName, directoryName))
                {
                    return (SinglePage)allFileModels.FirstOrDefault(x => x.Level == 1);
                }

                var directoryInfo = new DirectoryInfo(directoryName);
                var indexPageFileInfo = GetListPartialPageFileInfo(directoryInfo.Parent.FullName);

                if (indexPageFileInfo != null)
                {
                    return (SinglePage)allFileModels.FirstOrDefault(x => x.FullName.Equals(indexPageFileInfo.FullName, StringComparison.OrdinalIgnoreCase));
                }

                var supportedFiles = directoryInfo.Parent.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
                    .Where(x => IsSupportedPageFile(x.Name))
                    .ToList();

                foreach (var fi in supportedFiles)
                {
                    if (IsIndexPage(fi.Name) || IsListPartialPageFile(fi.Name))
                    {
                        return (SinglePage)allFileModels.FirstOrDefault(x => x.FullName.Equals(fi.FullName, StringComparison.OrdinalIgnoreCase));
                    }

                    ////if (supportedFiles.Count == 1)
                    ////{
                    ////    return (SinglePage)allFileModels.FirstOrDefault(x => x.FullName.Equals(fi.FullName, StringComparison.OrdinalIgnoreCase));
                    ////}
                }

                if (IsSameDirectory(this.contentRootDirectoryInfo.FullName, directoryInfo.Parent.FullName))
                {
                    return (SinglePage)allFileModels.FirstOrDefault(x => x.Level == 1);
                }

                directoryName = directoryInfo.Parent.FullName;
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

                if (GetListPartialPageFileInfo(directoryName) != null)
                {
                    nestedSection = directoryInfo.FullName;
                }

                // ReSharper disable once PossibleNullReferenceException
                directoryName = directoryInfo.Parent.FullName;

                if (IsSameDirectory(this.contentRootDirectoryInfo.FullName, directoryName))
                {
                    nestedSection = this.contentRootDirectoryInfo.FullName;
                }
            }

            return nestedSection;
        }
    }
}
