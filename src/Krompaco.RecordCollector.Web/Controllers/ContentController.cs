using System.Diagnostics;
using System.Globalization;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using Krompaco.RecordCollector.Content.IO;
using Krompaco.RecordCollector.Content.Languages;
using Krompaco.RecordCollector.Content.Models;
using Krompaco.RecordCollector.Web.Extensions;
using Krompaco.RecordCollector.Web.ModelBuilders;
using Krompaco.RecordCollector.Web.Models;
using Krompaco.RecordCollector.Web.Resources;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;

namespace Krompaco.RecordCollector.Web.Controllers
{
    public class ContentController : Controller
    {
        public const string AllFilesCacheKey = "RecordCollectorAllFiles";

        public static readonly object AllFilesLock = new ();

        private readonly ILogger<ContentController> logger;

        private readonly IStringLocalizer localizer;

        private readonly IWebHostEnvironment env;

        private readonly IConfiguration config;

        private readonly ContentCultureService contentCultureService;

        private readonly List<CultureInfo> rootCultures;

        private readonly string[] allFiles;

        private readonly string contentRoot;

        private readonly List<SinglePage> pagesForNavigation = new ();

        private readonly List<IRecordCollectorFile> allFileModels;

        private readonly Stopwatch stopwatch;

        private CultureInfo currentCulture;

        public ContentController(ILogger<ContentController> logger, IConfiguration config, IMemoryCache memoryCache, IWebHostEnvironment env, IStringLocalizerFactory factory)
        {
            this.logger = logger;
            this.config = config;
            this.env = env;

            this.localizer = factory.Create(typeof(SharedResource));

            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();

            this.contentRoot = this.config.GetAppSettingsContentRootPath();
            this.contentCultureService = new ContentCultureService();
            var fileService = new FileService(this.contentRoot, this.config.GetAppSettingsSectionsToExcludeFromLists(), this.contentCultureService, logger);
            this.rootCultures = fileService.GetRootCultures();
            this.currentCulture = CultureInfo.CurrentCulture;
            this.allFiles = fileService.GetAllFileFullNames();

            if (!memoryCache.TryGetValue(AllFilesCacheKey, out List<IRecordCollectorFile> allFileModelsFromCache))
            {
                lock (AllFilesLock)
                {
                    if (!memoryCache.TryGetValue(AllFilesCacheKey, out allFileModelsFromCache))
                    {
                        allFileModelsFromCache = fileService.GetAllFileModels();

                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromDays(1000))
                            .SetPriority(CacheItemPriority.High);

                        memoryCache.Set(AllFilesCacheKey, allFileModelsFromCache, cacheEntryOptions);
                    }
                }
            }

            this.allFileModels = allFileModelsFromCache;
        }

        [HttpGet]
        public IActionResult Report()
        {
            var sb = new StringBuilder();

            foreach (var fm in this.allFileModels.OrderBy(this.GetSortOrder))
            {
                var indent = new StringBuilder();

                for (var i = 0; i < fm.Level; i++)
                {
                    indent.Append("    ");
                }

                sb.AppendLine($"{indent}{fm.Title} {fm.FullName}");
                sb.AppendLine($"{indent}{fm.Level} {fm.GetType()} {fm.RelativeUrl}");
                sb.AppendLine($"{indent}Ancestors: {fm.Ancestors.Count}");
                sb.AppendLine($"{indent}Siblings: {fm.Siblings.Count}");
                sb.AppendLine($"{indent}Descendants: {fm.Descendants.Count}");
                sb.AppendLine($"{indent}ClosestSectionDirectory: {fm.ClosestSectionDirectory}");
                sb.AppendLine($"{indent}Section: {fm.Section}");
                sb.AppendLine($"{indent}Parent: {fm.ParentPage?.RelativeUrl.ToString() ?? "n/a"}");
                sb.AppendLine($"{indent}Previous: {fm.PreviousPage?.RelativeUrl.ToString() ?? "n/a"}");
                sb.AppendLine($"{indent}Next: {fm.NextPage?.RelativeUrl.ToString() ?? "n/a"}");

                if (fm is SinglePage)
                {
                    var sp = (SinglePage)fm;
                    sb.AppendLine($"{indent}Content length: {sp.Content?.Length ?? 0}");
                    sb.AppendLine($"{indent}Date: {sp.Date}");
                }

                sb.AppendLine();
            }

            this.LogTime();
            return this.Content(sb.ToString(), "text/plain", Encoding.UTF8);
        }

        [HttpGet]
        public IActionResult Properties()
        {
            var model = this.GetContentProperties();

            var json = this.Json(model);
            this.LogTime();
            return json;
        }

        [HttpGet]
        public IActionResult Files(string? path)
        {
            var siteUrl = this.config.GetAppSettingsSiteUrl();
            var viewPrefix = this.config.GetAppSettingsViewPrefix();
            var rqf = this.Request.HttpContext.Features.Get<IRequestCultureFeature>();
            this.currentCulture = rqf?.RequestCulture.Culture ?? CultureInfo.CurrentCulture;
            var contentProperties = this.GetContentProperties();
            this.logger.LogInformation($"Culture is {this.currentCulture.EnglishName} and local time is {DateTime.Now}.");

            // Check if RSS request
            var rssForList = path != null
                             && !string.IsNullOrWhiteSpace(siteUrl)
                             && path.EndsWith("rss.xml", StringComparison.OrdinalIgnoreCase);

            if (rssForList)
            {
                this.logger.LogInformation("RSS requested.");
                path = Regex.Replace(path ?? string.Empty, "rss.xml$", string.Empty, RegexOptions.IgnoreCase);
                this.logger.LogInformation($"Path now: \"{path}\"");
            }

            // Fix path for pagination
            var isPaginationPath = IsPaginationPath(path);
            path = RemovePaginationFromPath(path);

            // Main navigation
            var mainNavigationSections = this.config.GetAppSettingsMainNavigationSections();

            if (mainNavigationSections.Any())
            {
                if (this.rootCultures.Any() && !string.IsNullOrEmpty(path))
                {
                    var rangeToAdd = this.allFileModels
                        .Where(x =>
                            mainNavigationSections.Contains(x.Section ?? string.Empty)
                            && x.RelativeUrl
                                .ToString()
                                .TrimStart('/')
                                .StartsWith($"{this.currentCulture.Name}/", StringComparison.OrdinalIgnoreCase))
                        .Select(x => x as SinglePage)
                        .Where(x => x?.Title != null && x.Level == 1)
                        .OrderByDescending(x => x?.Weight ?? int.MinValue)
                        .ThenBy(x => x?.Title)
                        .ToList();

                    if (rangeToAdd.Any())
                    {
                        this.pagesForNavigation.AddRange(rangeToAdd!);
                    }
                }
                else if (!this.rootCultures.Any())
                {
#pragma warning disable IDE0007 // Use implicit type
                    List<SinglePage> rangeToAdd = this.allFileModels
#pragma warning restore IDE0007 // Use implicit type
                        .Where(x =>
                            mainNavigationSections.Contains(x.Section ?? string.Empty))
                        .Select(x => x as SinglePage)
                        .Where(x => x != null && x?.Title != null && x.Level == 1)
                        .OrderByDescending(x => x?.Weight ?? int.MinValue)
                        .ThenBy(x => x?.Title)
                        .ToList() !;

                    if (rangeToAdd.Any())
                    {
                        this.pagesForNavigation.AddRange(rangeToAdd);
                    }
                }
            }

            // Start page
            if (string.IsNullOrEmpty(path))
            {
                this.logger.LogInformation("Path is null or empty so must mean root/startpage.");

                var rootPage = this.allFileModels
                                   .Where(x => x.RelativeUrl.ToString() == "/")
                                   .Select(x => x as ListPage)
                                   .FirstOrDefault() ?? new ListPage();

                if (this.rootCultures.Any())
                {
                    if (isPaginationPath)
                    {
                        this.LogTime();
                        return this.NotFound();
                    }

                    var rootViewModel = new LayoutViewModelBuilder<ListPageViewModel, ListPage>(rootPage, this.currentCulture, this.rootCultures, this.Request, this.localizer, contentProperties)
                        .WithMarkdownPipeline()
                        .WithMeta()
                        .GetViewModel();

                    rootViewModel.Title = rootPage.Title ?? this.localizer["Select language"];

                    this.LogTime();
                    return this.View(viewPrefix + "List", rootViewModel);
                }

                var listViewModel = new LayoutViewModelBuilder<ListPageViewModel, ListPage>(rootPage, this.currentCulture, this.rootCultures, this.Request, this.localizer, contentProperties)
                    .WithMarkdownPipeline()
                    .WithMeta()
                    .WithPaginationItems(
                        this.config.GetAppSettingsPaginationPageCount(),
                        this.config.GetAppSettingsPaginationPageSize())
                    .WithNavigationItems(this.pagesForNavigation)
                    .GetViewModel();

                if (isPaginationPath && listViewModel?.PagedDescendantPages?.Count == 0)
                {
                    this.LogTime();
                    return this.NotFound();
                }

                listViewModel!.Title = rootPage.Title ?? this.localizer["Updates"];

                if (rssForList)
                {
                    return this.GetXmlActionResultAndLogTime(rootPage, listViewModel, siteUrl, false);
                }

                this.LogTime();
                return this.View(viewPrefix + "List", listViewModel);
            }

            var items = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (items.Length == 1)
            {
                var firstDirectoryInPath = items[0];

                this.logger.LogInformation($"First directory in path: {firstDirectoryInPath}");

                var doesCultureExist = this.contentCultureService.DoesCultureExist(firstDirectoryInPath);

                if (doesCultureExist)
                {
                    var cultureInfo = new CultureInfo(firstDirectoryInPath);
                    this.logger.LogInformation($"URL part {firstDirectoryInPath} was found as {cultureInfo.EnglishName} culture.");

                    var listPage = this.allFileModels
                                       .Where(x => x.RelativeUrl.ToString() == "/" + cultureInfo.Name + "/")
                                       .Select(x => x as ListPage)
                                       .FirstOrDefault() ?? new ListPage();

                    var listViewModel = new LayoutViewModelBuilder<ListPageViewModel, ListPage>(listPage, this.currentCulture, this.rootCultures, this.Request, this.localizer, contentProperties)
                        .WithMarkdownPipeline()
                        .WithMeta()
                        .WithPaginationItems(
                            this.config.GetAppSettingsPaginationPageCount(),
                            this.config.GetAppSettingsPaginationPageSize())
                        .WithNavigationItems(this.pagesForNavigation)
                        .GetViewModel();

                    if (isPaginationPath && listViewModel?.PagedDescendantPages?.Count == 0)
                    {
                        this.LogTime();
                        return this.NotFound();
                    }

                    listViewModel!.Title = listPage.Title ?? cultureInfo.NativeName.FirstCharToUpper();

                    if (rssForList)
                    {
                        return this.GetXmlActionResultAndLogTime(listPage, listViewModel, siteUrl, false);
                    }

                    this.LogTime();
                    return this.View(viewPrefix + "List", listViewModel);
                }
            }

            var physicalPath = path.Replace('/', Path.DirectorySeparatorChar);

            // File with extension
            if (!path.EndsWith('/')
                && path.Contains('.', StringComparison.Ordinal))
            {
                var foundFullName = this.allFiles.FirstOrDefault(x => x.EndsWith(physicalPath, StringComparison.InvariantCultureIgnoreCase));

                if (foundFullName == null)
                {
                    this.LogTime();
                    return this.NotFound();
                }

                var contentTypeProvider = new FileExtensionContentTypeProvider();
                contentTypeProvider.TryGetContentType(foundFullName, out var contentType);

                this.LogTime();
                return this.PhysicalFile(foundFullName, contentType ?? "text/plain");
            }

            // Some kind of page
            physicalPath = physicalPath.TrimEnd(Path.DirectorySeparatorChar) + ".md";
            this.logger.LogInformation($"Lookup by {physicalPath}");
            var foundPage = this.allFiles.FirstOrDefault(x => x.EndsWith(physicalPath, StringComparison.OrdinalIgnoreCase));

            if (foundPage == null)
            {
                physicalPath = physicalPath.Replace(".md", ".html", StringComparison.OrdinalIgnoreCase);
                this.logger.LogInformation($"Lookup by {physicalPath}");
                foundPage = this.allFiles.FirstOrDefault(x => x.EndsWith(physicalPath, StringComparison.OrdinalIgnoreCase));
            }

            if (foundPage == null)
            {
                physicalPath = physicalPath.Replace(".html", Path.DirectorySeparatorChar + "index.html", StringComparison.OrdinalIgnoreCase);
                this.logger.LogInformation($"Lookup by {physicalPath}");
                foundPage = this.allFiles.FirstOrDefault(x => x.EndsWith(physicalPath, StringComparison.OrdinalIgnoreCase));
            }

            if (foundPage == null)
            {
                physicalPath = physicalPath.Replace(".html", ".md", StringComparison.OrdinalIgnoreCase);
                this.logger.LogInformation($"Lookup by {physicalPath}");
                foundPage = this.allFiles.FirstOrDefault(x => x.EndsWith(physicalPath, StringComparison.OrdinalIgnoreCase));
            }

            if (foundPage == null)
            {
                physicalPath = physicalPath.Replace("index.md", "_index.md", StringComparison.OrdinalIgnoreCase);
                this.logger.LogInformation($"Lookup by {physicalPath}");
                foundPage = this.allFiles.FirstOrDefault(x => x.EndsWith(physicalPath, StringComparison.OrdinalIgnoreCase));
            }

            if (foundPage == null)
            {
                physicalPath = physicalPath.Replace(".md", ".html", StringComparison.OrdinalIgnoreCase);
                this.logger.LogInformation($"Lookup by {physicalPath}");
                foundPage = this.allFiles.FirstOrDefault(x => x.EndsWith(physicalPath, StringComparison.OrdinalIgnoreCase));
            }

            if (foundPage == null)
            {
                this.LogTime();
                return this.NotFound();
            }

            // Category listing check
            if (this.allFileModels.FirstOrDefault(x => x.FullName.Equals(foundPage, StringComparison.OrdinalIgnoreCase)) is ListPage listPageForCategory && !string.IsNullOrWhiteSpace(listPageForCategory.ListCategory))
            {
                var listViewModel = new LayoutViewModelBuilder<ListPageViewModel, ListPage>(listPageForCategory, this.currentCulture, this.rootCultures, this.Request, this.localizer, contentProperties)
                    .WithMarkdownPipeline()
                    .WithMeta()
                    .WithPaginationItems(
                        this.config.GetAppSettingsPaginationPageCount(),
                        this.config.GetAppSettingsPaginationPageSize())
                    .WithNavigationItems(this.pagesForNavigation)
                    .GetViewModel();

                if (isPaginationPath && listViewModel?.PagedDescendantPages?.Count == 0)
                {
                    this.LogTime();
                    return this.NotFound();
                }

                listViewModel!.Title = listPageForCategory.Title;

                if (rssForList)
                {
                    return this.GetXmlActionResultAndLogTime(listPageForCategory, listViewModel, siteUrl, true);
                }

                this.LogTime();
                return this.View(viewPrefix + "List", listViewModel);
            }

            // Single page
            if (!(this.allFileModels.FirstOrDefault(x => x.FullName.Equals(foundPage, StringComparison.OrdinalIgnoreCase)) is SinglePage singlePage))
            {
                this.LogTime();
                return this.NotFound();
            }

            var singleViewModel = new LayoutViewModelBuilder<SinglePageViewModel, SinglePage>(singlePage, this.currentCulture, this.rootCultures, this.Request, this.localizer, contentProperties)
                .WithMarkdownPipeline()
                .WithMeta()
                .WithNavigationItems(this.pagesForNavigation)
                .GetViewModel();

            singleViewModel.Categories = new List<CategoryItemViewModel>();

            if (singlePage.Categories is { Count: > 0 })
            {
                foreach (var category in singlePage.Categories)
                {
                    var (url, count) = this.GetUrlAndCountForCategoryList(category);

                    singleViewModel.Categories.Add(new CategoryItemViewModel
                    {
                        RelativeUrl = url,
                        Text = category,
                        PageCount = count,
                    });
                }
            }

            this.LogTime();
            return this.View(viewPrefix + "Single", singleViewModel);
        }

        private static string RemovePaginationFromPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            path = path.TrimStart('/');
            path = "/" + path;

            return Regex.Replace(path, "/page-\\d+/$", string.Empty, RegexOptions.IgnoreCase);
        }

        private static bool IsPaginationPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            path = path.TrimStart('/');
            path = "/" + path;

            var match = Regex.Match(path, "/page-\\d+/$", RegexOptions.IgnoreCase);

            return match.Success && match.Groups.Count > 0;
        }

        private Tuple<Uri?, int> GetUrlAndCountForCategoryList(string category)
        {
            ListPage? firstCategoryListPage = null;

            if (this.rootCultures.Any())
            {
                firstCategoryListPage = this.allFileModels
                    .Where(x => x.RelativeUrl
                            .ToString()
                            .TrimStart('/')
                            .StartsWith($"{this.currentCulture.Name}/", StringComparison.OrdinalIgnoreCase))
                    .Select(x => x as ListPage)
                    .FirstOrDefault(x => x?.ListCategory != null
                        && x.ListCategory.Equals(category, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                firstCategoryListPage = this.allFileModels
                    .Select(x => x as ListPage)
                    .FirstOrDefault(x => x?.ListCategory != null
                                         && x.ListCategory.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            return firstCategoryListPage != null ?
                new Tuple<Uri?, int>(firstCategoryListPage.RelativeUrl, firstCategoryListPage.CategoryPages.Count)
                : new Tuple<Uri?, int>(null, 0);
        }

        private ContentProperties GetContentProperties()
        {
            var model = new ContentProperties
            {
                ContentRootPath = this.contentRoot,
                StaticSiteRootPath = this.config.GetAppSettingsStaticSiteRootPath(),
                SectionsToExcludeFromLists = this.config.GetAppSettingsSectionsToExcludeFromLists(),
                EnvironmentProjectWebRootPath = this.env.WebRootPath,
                SiteUrl = this.config.GetAppSettingsSiteUrl(),
                FrontendSetup = this.config.GetAppSettingsFrontendSetup(),
            };

            return model;
        }

        private int GetSortOrder(IRecordCollectorFile file)
        {
            if (file.GetType() == typeof(ListPage))
            {
                return 0;
            }

            if (file.GetType() == typeof(SinglePage))
            {
                return 1;
            }

            return 2;
        }

        private IActionResult GetXmlActionResultAndLogTime(ListPage listPage, LayoutViewModel viewModel, string siteUrl, bool isCategory)
        {
            var rssUrl = new Uri(
                new Uri(siteUrl),
                $"{listPage.RelativeUrl}rss.xml");
            var rssItems = isCategory ? listPage.CategoryPages.Take(10).ToList() : listPage.DescendantPages.Take(10).ToList();
            var lastUpdatedTime = rssItems.Any() ? rssItems.First().Date.ToUniversalTime() : DateTime.UtcNow;
            var lastUpdatedTimeOffset = new DateTimeOffset(lastUpdatedTime);
            var rssXmlBuilder = new RssXmlBuilder(
                new Uri(siteUrl),
                this,
                rssItems,
                new SyndicationFeed(
                    viewModel.Title,
                    viewModel.Description,
                    rssUrl,
                    rssUrl.ToString(),
                    lastUpdatedTimeOffset));

            var result = rssXmlBuilder.BuildActionResult();

            this.LogTime();

            return result;
        }

        private void LogTime()
        {
            this.stopwatch.Stop();
            this.logger.LogInformation($"Time in controller: {this.stopwatch.Elapsed.TotalMilliseconds} ms");
        }
    }
}
