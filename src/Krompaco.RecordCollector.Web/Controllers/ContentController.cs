using System;
using System.IO;
using System.Linq;
using Krompaco.RecordCollector.Content.IO;
using Krompaco.RecordCollector.Content.Models;
using Krompaco.RecordCollector.Web.Extensions;
using Krompaco.RecordCollector.Web.ModelBuilders;
using Krompaco.RecordCollector.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Krompaco.RecordCollector.Web.Controllers
{
    public class ContentController : Controller
    {
        private readonly ILogger<ContentController> logger;

        private readonly IConfiguration config;

        private readonly FileService fileService;

        private readonly string[] allFiles;

        public ContentController(ILogger<ContentController> logger, IConfiguration config)
        {
            this.logger = logger;
            this.config = config;
            this.fileService = new FileService(this.config.GetAppSettingsContentRootPath());
            this.allFiles = this.fileService.GetAllFileFullNames();
        }

        [HttpGet]
        public IActionResult Files(string path)
        {
            // Start page
            if (path == null)
            {
                var list = this.allFiles
                    .Where(x => x.Contains(".md", StringComparison.OrdinalIgnoreCase) &&
                                !x.Contains("index.md", StringComparison.OrdinalIgnoreCase))
                    .Select(x => this.fileService.GetAsFileModel(x) as SinglePage)
                    .Where(x => x != null)
                    .ToList();

                var listPage = new ListPage
                {
                    Children = list,
                };

                var listViewModel = new LayoutViewModelBuilder<ListPageViewModel, ListPage>(listPage)
                    .WithMarkdownPipeline()
                    .WithMeta()
                    .GetViewModel();

                listViewModel.Title = "Welcome";

                return this.View("List", listViewModel);
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

            // Page
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

            var singleViewModel = new LayoutViewModelBuilder<SinglePageViewModel, SinglePage>(singlePage)
                .WithMarkdownPipeline()
                .WithMeta()
                .GetViewModel();

            return this.View("Single", singleViewModel);
        }
    }
}
