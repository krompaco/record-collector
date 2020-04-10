using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Krompaco.RecordCollector.Content.IO;
using Krompaco.RecordCollector.Content.Languages;
using Krompaco.RecordCollector.Content.Models;
using Krompaco.RecordCollector.Web.Extensions;
using Krompaco.RecordCollector.Web.ModelBuilders;
using Krompaco.RecordCollector.Web.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Krompaco.RecordCollector.Web.Controllers
{
    public class ContentController : Controller
    {
        public const string AllFilesCacheKey = "RecordCollectorAllFiles";

        public static readonly object AllFilesLock = new object();

        private readonly ILogger<ContentController> logger;

        private readonly FileService fileService;

        private readonly ContentCultureService contentCultureService;

        private readonly List<CultureInfo> rootCultures;

        private readonly string[] allFiles;

        private readonly string contentRoot;

        private readonly List<SinglePage> pagesForNavigation;

        private readonly List<IFile> allFileModels;

        private readonly Stopwatch stopwatch;

        public ContentController(ILogger<ContentController> logger, IConfiguration config, IMemoryCache memoryCache)
        {
            this.logger = logger;

            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();

            this.contentRoot = config.GetAppSettingsContentRootPath();
            this.contentCultureService = new ContentCultureService();
            this.fileService = new FileService(this.contentRoot, this.contentCultureService, logger);
            this.rootCultures = this.fileService.GetRootCultures();
            this.allFiles = this.fileService.GetAllFileFullNames();

            if (!memoryCache.TryGetValue(AllFilesCacheKey, out List<IFile> allFileModelsFromCache))
            {
                lock (AllFilesLock)
                {
                    if (!memoryCache.TryGetValue(AllFilesCacheKey, out allFileModelsFromCache))
                    {
                        allFileModelsFromCache = this.fileService.GetAllFileModels();

                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromDays(1000))
                            .SetPriority(CacheItemPriority.High);

                        memoryCache.Set(AllFilesCacheKey, allFileModelsFromCache, cacheEntryOptions);
                    }
                }
            }

            this.allFileModels = allFileModelsFromCache;

            this.pagesForNavigation = new List<SinglePage>();
        }

        [HttpGet]
        public IActionResult Table(string path)
        {
            this.LogTime();
            return this.Json(this.allFileModels);
        }

        [HttpGet]
        public IActionResult Files(string path)
        {
            var rqf = this.Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture.Culture;
            this.logger.LogInformation($"Culture is {culture.EnglishName} and local time is {DateTime.Now}.");

            this.pagesForNavigation.AddRange(this.allFileModels
                .Where(x => x.Section == "page")
                .Select(x => x as SinglePage)
                .Where(x => x?.Title != null)
                .OrderByDescending(x => x.Weight)
                .ThenBy(x => x.Title)
                .ToList());

            // Start page
            if (path == null)
            {
                this.logger.LogInformation("Path is null so must mean root/startpage.");

                if (this.rootCultures.Any())
                {
                    var rootIndexPage = this.fileService.GetIndexPageFullName(this.contentRoot);
                    ListPage rootPage;

                    if (rootIndexPage != null)
                    {
                        rootPage = this.allFileModels
                                       .Where(x => x.FullName.Equals(rootIndexPage.FullName, StringComparison.OrdinalIgnoreCase)
                                                   && x is ListPage)
                                       .Select(x => x as ListPage)
                                       .FirstOrDefault() ?? new ListPage();
                    }
                    else
                    {
                        rootPage = new ListPage();
                    }

                    var rootViewModel = new LayoutViewModelBuilder<ListPageViewModel, ListPage>(rootPage, culture, this.rootCultures)
                        .WithMarkdownPipeline()
                        .WithMeta(this.Request)
                        .WithNavigationItems(this.Request, this.pagesForNavigation)
                        .GetViewModel();

                    rootViewModel.Title = rootPage.Title ?? "Select Language";
                    rootViewModel.CurrentPage.ChildPages = new List<SinglePage>();

                    this.LogTime();
                    return this.View("List", rootViewModel);
                }

                var list = this.allFileModels
                    .OfType<SinglePage>()
                    .Where(x => x?.Title != null
                                && !this.fileService.IsListPartialPageFile(x.FullName)
                                && !x.Section.Equals("page", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.Date)
                    .ToList();

                var indexPage = this.fileService.GetIndexPageFullName(this.contentRoot);

                ListPage listPage;

                if (indexPage != null)
                {
                    listPage = this.allFileModels
                                   .Where(x => x.FullName.Equals(indexPage.FullName, StringComparison.OrdinalIgnoreCase)
                                               && x is ListPage)
                                   .Select(x => x as ListPage)
                                   .FirstOrDefault() ?? new ListPage();
                }
                else
                {
                    listPage = new ListPage();
                }

                listPage.ChildPages = list;

                var listViewModel = new LayoutViewModelBuilder<ListPageViewModel, ListPage>(listPage, culture, this.rootCultures)
                    .WithMarkdownPipeline()
                    .WithMeta(this.Request)
                    .WithNavigationItems(this.Request, this.pagesForNavigation)
                    .GetViewModel();

                listViewModel.Title = listPage.Title ?? "Posts";

                this.LogTime();
                return this.View("List", listViewModel);
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
                    var cultureRootPath = Path.Combine(this.contentRoot, firstDirectoryInPath);

                    var list = this.allFileModels
                        .Where(x => x is SinglePage
                                    && x.FullName.StartsWith(cultureRootPath, StringComparison.OrdinalIgnoreCase)
                                    && !this.fileService.IsListPartialPageFile(x.FullName)
                                    && !x.Section.Equals("page", StringComparison.OrdinalIgnoreCase))
                        .Select(x => x as SinglePage)
                        .OrderByDescending(x => x.Date)
                        .ToList();

                    var indexPage = this.fileService.GetIndexPageFullName(cultureRootPath);
                    ListPage listPage;

                    if (indexPage != null)
                    {
                        listPage = this.allFileModels
                                       .Where(x => x.FullName.Equals(indexPage.FullName, StringComparison.OrdinalIgnoreCase)
                                                   && x is ListPage)
                                       .Select(x => x as ListPage)
                                       .FirstOrDefault() ?? new ListPage();
                    }
                    else
                    {
                        listPage = new ListPage();
                    }

                    listPage.ChildPages = list;

                    var listViewModel = new LayoutViewModelBuilder<ListPageViewModel, ListPage>(listPage, culture, this.rootCultures)
                        .WithMarkdownPipeline()
                        .WithMeta(this.Request)
                        .WithNavigationItems(this.Request, this.pagesForNavigation)
                        .GetViewModel();

                    listViewModel.Title = cultureInfo.NativeName;

                    this.LogTime();
                    return this.View("List", listViewModel);
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
                return this.PhysicalFile(foundFullName, contentType);
            }

            // Post
            physicalPath = physicalPath.TrimEnd(Path.DirectorySeparatorChar) + ".md";
            var foundPage = this.allFiles.FirstOrDefault(x => x.EndsWith(physicalPath, StringComparison.OrdinalIgnoreCase));

            if (foundPage == null)
            {
                physicalPath = physicalPath.Replace(".md", ".html", StringComparison.OrdinalIgnoreCase);
                foundPage = this.allFiles.FirstOrDefault(x => x.EndsWith(physicalPath, StringComparison.OrdinalIgnoreCase));
            }

            if (foundPage == null)
            {
                this.LogTime();
                return this.NotFound();
            }

            if (!(this.allFileModels.FirstOrDefault(x => x.FullName.Equals(foundPage, StringComparison.OrdinalIgnoreCase)) is SinglePage singlePage))
            {
                this.LogTime();
                return this.NotFound();
            }

            var singleViewModel = new LayoutViewModelBuilder<SinglePageViewModel, SinglePage>(singlePage, culture, this.rootCultures)
                .WithMarkdownPipeline()
                .WithMeta(this.Request)
                .WithNavigationItems(this.Request, this.pagesForNavigation)
                .GetViewModel();

            this.LogTime();
            return this.View("Single", singleViewModel);
        }

        private void LogTime()
        {
            this.stopwatch.Stop();
            this.logger.LogInformation($"Time in controller: {this.stopwatch.Elapsed.TotalMilliseconds} ms");
        }
    }
}
