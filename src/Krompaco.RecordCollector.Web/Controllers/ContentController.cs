using System.IO;
using Krompaco.RecordCollector.Content.IO;
using Krompaco.RecordCollector.Web.Extensions;
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

        public ContentController(ILogger<ContentController> logger, IConfiguration config)
        {
            this.logger = logger;
            this.config = config;
            this.fileService = new FileService(this.config.GetAppSettingsContentRootPath());
        }

        [HttpGet]
        public IActionResult Files(string path)
        {
            var filesPath = Path.Combine("files", path);
            var physicalPath = this.fileService.GetPhysicalPath(filesPath);
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.TryGetContentType(physicalPath, out var contentType);
            return this.PhysicalFile(physicalPath, contentType);
        }

        [HttpGet]
        public IActionResult Images(string path)
        {
            var filesPath = Path.Combine("images", path);
            var physicalPath = this.fileService.GetPhysicalPath(filesPath);
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.TryGetContentType(physicalPath, out var contentType);
            return this.PhysicalFile(physicalPath, contentType);
        }
    }
}
