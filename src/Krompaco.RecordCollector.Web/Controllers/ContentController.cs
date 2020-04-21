using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Krompaco.RecordCollector.Content.IO;
using Krompaco.RecordCollector.Content.Languages;
using Krompaco.RecordCollector.Content.Models;
using Krompaco.RecordCollector.Web.Extensions;
using Krompaco.RecordCollector.Web.ModelBuilders;
using Krompaco.RecordCollector.Web.Models;
using Microsoft.AspNetCore.Hosting;
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

        private readonly IWebHostEnvironment env;

        private readonly IConfiguration config;

        private readonly ContentCultureService contentCultureService;

        private readonly List<CultureInfo> rootCultures;

        private readonly string[] allFiles;

        private readonly string contentRoot;

        private readonly List<SinglePage> pagesForNavigation;

        private readonly List<IRecordCollectorFile> allFileModels;

        private readonly Stopwatch stopwatch;

        public ContentController(ILogger<ContentController> logger, IConfiguration config, IMemoryCache memoryCache, IWebHostEnvironment env)
        {
            this.logger = logger;
            this.config = config;
            this.env = env;

            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();

            this.contentRoot = this.config.GetAppSettingsContentRootPath();
            this.contentCultureService = new ContentCultureService();
            var fileService = new FileService(this.contentRoot, this.config.GetAppSettingsSectionsToExcludeFromLists(), this.contentCultureService, logger);
            this.rootCultures = fileService.GetRootCultures();
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

            this.pagesForNavigation = new List<SinglePage>();
        }

        [HttpGet]
        public IActionResult Report()
        {
            this.LogTime();
            var sb = new StringBuilder();

            foreach (var fm in this.allFileModels)
            {
                sb.AppendLine($"{fm.Title} {fm.FullName}");
                sb.AppendLine($"{fm.Level} {fm.GetType()} {fm.RelativeUrl}");
                sb.AppendLine($"Ancestors: {fm.Ancestors.Count}");
                sb.AppendLine($"Siblings: {fm.Siblings.Count}");
                sb.AppendLine($"Descendants: {fm.Descendants.Count}");
                sb.AppendLine($"ClosestSectionDirectory: {fm.ClosestSectionDirectory}");
                sb.AppendLine($"Section: {fm.Section}");
                sb.AppendLine($"Parent: {fm.Parent?.RelativeUrl.ToString() ?? "n/a"}");

                if (fm is SinglePage)
                {
                    var sp = (SinglePage)fm;
                    sb.AppendLine($"Content length: {sp.Content?.Length ?? 0}");
                    sb.AppendLine($"Date: {sp.Date}");
                }

                sb.AppendLine();
            }

            return this.Content(sb.ToString(), "text/plain");
        }

        [HttpGet]
        public IActionResult Properties()
        {
            this.LogTime();

            var model = new ContentProperties
            {
                ContentRootPath = this.contentRoot,
                StaticSiteRootPath = this.config.GetAppSettingsStaticSiteRootPath(),
                SectionsToExcludeFromLists = this.config.GetAppSettingsSectionsToExcludeFromLists(),
                EnvironmentProjectWebRootPath = this.env.WebRootPath,
            };

            return this.Json(model);
        }

        [HttpGet]
        public IActionResult Files(string path)
        {
            var rqf = this.Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture.Culture;
            this.logger.LogInformation($"Culture is {culture.EnglishName} and local time is {DateTime.Now}.");

            // Main navigation
            var mainNavigationSection = this.config.GetAppSettingsMainNavigationSection();

            if (mainNavigationSection != null)
            {
                this.pagesForNavigation.AddRange(this.allFileModels
                    .Where(x => x.Section == mainNavigationSection)
                    .Select(x => x as SinglePage)
                    .Where(x => x?.Title != null)
                    .OrderByDescending(x => x.Weight)
                    .ThenBy(x => x.Title)
                    .ToList());
            }

            // Start page
            if (path == null)
            {
                this.logger.LogInformation("Path is null so must mean root/startpage.");

                var rootPage = this.allFileModels
                                   .Where(x => x.RelativeUrl.ToString() == "/")
                                   .Select(x => x as ListPage)
                                   .FirstOrDefault() ?? new ListPage();

                if (this.rootCultures.Any())
                {
                    var rootViewModel = new LayoutViewModelBuilder<ListPageViewModel, ListPage>(rootPage, culture, this.rootCultures)
                        .WithMarkdownPipeline()
                        .WithMeta(this.Request)
                        .WithNavigationItems(this.Request, this.pagesForNavigation)
                        .GetViewModel();

                    rootViewModel.Title = rootPage.Title ?? "Select Language";
                    rootViewModel.CurrentPage.DescendantPages = new List<SinglePage>();

                    this.LogTime();
                    return this.View("List", rootViewModel);
                }

                var listViewModel = new LayoutViewModelBuilder<ListPageViewModel, ListPage>(rootPage, culture, this.rootCultures)
                    .WithMarkdownPipeline()
                    .WithMeta(this.Request)
                    .WithNavigationItems(this.Request, this.pagesForNavigation)
                    .GetViewModel();

                listViewModel.Title = rootPage.Title ?? "Posts";

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

                    var listPage = this.allFileModels
                                       .Where(x => x.RelativeUrl.ToString() == "/" + cultureInfo.Name + "/")
                                       .Select(x => x as ListPage)
                                       .FirstOrDefault() ?? new ListPage();

                    var listViewModel = new LayoutViewModelBuilder<ListPageViewModel, ListPage>(listPage, culture, this.rootCultures)
                        .WithMarkdownPipeline()
                        .WithMeta(this.Request)
                        .WithNavigationItems(this.Request, this.pagesForNavigation)
                        .GetViewModel();

                    listViewModel.Title = listPage.Title ?? cultureInfo.NativeName;

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
