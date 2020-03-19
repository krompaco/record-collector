using Krompaco.RecordCollector.Content.IO;
using Krompaco.RecordCollector.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Krompaco.RecordCollector.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;

        private readonly IConfiguration config;

        private readonly FileService fileService;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            this.logger = logger;
            this.config = config;

            this.fileService = new FileService(this.config.GetAppSettingsContentRootPath());
        }

        public IActionResult Index()
        {
            var allFiles = this.fileService.GetAllFileFullNames();
            return this.View();
        }
    }
}
