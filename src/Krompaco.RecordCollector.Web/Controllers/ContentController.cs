using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Krompaco.RecordCollector.Web.Controllers
{
    public class ContentController : Controller
    {
        private readonly ILogger<ContentController> logger;

        private readonly FileService fileService;

        private readonly ContentCultureService contentCultureService;

        private readonly string[] allFiles;

        private readonly string contentRoot;

        private readonly List<SinglePage> pagesForNavigation;

        public ContentController(ILogger<ContentController> logger, IConfiguration config)
        {
            this.logger = logger;
            this.contentRoot = config.GetAppSettingsContentRootPath();
            this.contentCultureService = new ContentCultureService();
            this.fileService = new FileService(this.contentRoot, this.contentCultureService);
            this.allFiles = this.fileService.GetAllFileFullNames();
            this.pagesForNavigation = new List<SinglePage>();
        }

        [HttpGet]
        public IActionResult Files(string path)
        {
            var rqf = this.Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture.Culture;
            this.logger.LogInformation($"Culture is {culture.EnglishName} and local time is {DateTime.Now}.");
            var rootCultures = this.fileService.GetRootCultures();

            this.pagesForNavigation.AddRange(this.allFiles
                .Where(x => (x.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                             || x.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                            && this.fileService.GetSectionFromFullName(x) == "page")
                .Select(x => this.fileService.GetAsFileModel(x) as SinglePage)
                .Where(x => x?.Title != null)
                .OrderByDescending(x => x.Weight)
                .ThenBy(x => x.Title)
                .ToList());

            // Start page
            if (path == null)
            {
                this.logger.LogInformation("Path is null so must mean root/startpage.");

                if (rootCultures.Any())
                {
                    var rootIndexPage = this.fileService.GetIndexPageFullName(this.contentRoot);
                    ListPage rootPage;

                    if (rootIndexPage != null)
                    {
                        rootPage = this.fileService.GetAsFileModel(rootIndexPage.FullName) as ListPage ?? new ListPage();
                    }
                    else
                    {
                        rootPage = new ListPage();
                    }

                    var rootViewModel = new LayoutViewModelBuilder<ListPageViewModel, ListPage>(rootPage, culture, rootCultures)
                        .WithMarkdownPipeline()
                        .WithMeta(this.Request)
                        .WithNavigationItems(this.Request, this.pagesForNavigation)
                        .GetViewModel();

                    rootViewModel.Title = rootPage.Title ?? "Root";
                    rootViewModel.CurrentPage.ChildPages = new List<SinglePage>();

                    return this.View("List", rootViewModel);
                }

                var list = this.allFiles
                    .Where(x => (x.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                                    || x.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                                && !x.EndsWith("_index.md", StringComparison.OrdinalIgnoreCase)
                                && !x.EndsWith("_index.html", StringComparison.OrdinalIgnoreCase))
                    .Select(x => this.fileService.GetAsFileModel(x) as SinglePage)
                    .Where(x => x?.Title != null
                                && !x.Section.Equals("page", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.Date)
                    .ToList();

                var indexPage = this.fileService.GetIndexPageFullName(this.contentRoot);
                ListPage listPage;

                if (indexPage != null)
                {
                    listPage = this.fileService.GetAsFileModel(indexPage.FullName) as ListPage ?? new ListPage();
                }
                else
                {
                    listPage = new ListPage();
                }

                listPage.ChildPages = list;

                var listViewModel = new LayoutViewModelBuilder<ListPageViewModel, ListPage>(listPage, culture, rootCultures)
                    .WithMarkdownPipeline()
                    .WithMeta(this.Request)
                    .WithNavigationItems(this.Request, this.pagesForNavigation)
                    .GetViewModel();

                listViewModel.Title = listPage.Title ?? "Root";

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

                    var list = this.allFiles
                        .Where(x => x.StartsWith(cultureRootPath, StringComparison.OrdinalIgnoreCase)
                                    && (x.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                                        || x.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                                    && !x.EndsWith("_index.md", StringComparison.OrdinalIgnoreCase)
                                    && !x.EndsWith("_index.html", StringComparison.OrdinalIgnoreCase))
                        .Select(x => this.fileService.GetAsFileModel(x) as SinglePage)
                        .Where(x => x != null
                                    && !x.Section.Equals("page", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(x => x.Date)
                        .ToList();

                    var indexPage = this.fileService.GetIndexPageFullName(cultureRootPath);
                    ListPage listPage;

                    if (indexPage != null)
                    {
                        listPage = this.fileService.GetAsFileModel(indexPage.FullName) as ListPage ?? new ListPage();
                    }
                    else
                    {
                        listPage = new ListPage();
                    }

                    listPage.ChildPages = list;

                    var listViewModel = new LayoutViewModelBuilder<ListPageViewModel, ListPage>(listPage, culture, rootCultures)
                        .WithMarkdownPipeline()
                        .WithMeta(this.Request)
                        .WithNavigationItems(this.Request, this.pagesForNavigation)
                        .GetViewModel();

                    listViewModel.Title = cultureInfo.NativeName;

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
                    return this.NotFound();
                }

                var contentTypeProvider = new FileExtensionContentTypeProvider();
                contentTypeProvider.TryGetContentType(foundFullName, out var contentType);
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
                return this.NotFound();
            }

            if (!(this.fileService.GetAsFileModel(foundPage) is SinglePage singlePage))
            {
                return this.NotFound();
            }

            var singleViewModel = new LayoutViewModelBuilder<SinglePageViewModel, SinglePage>(singlePage, culture, rootCultures)
                .WithMarkdownPipeline()
                .WithMeta(this.Request)
                .WithNavigationItems(this.Request, this.pagesForNavigation)
                .GetViewModel();

            return this.View("Single", singleViewModel);
        }
    }
}
