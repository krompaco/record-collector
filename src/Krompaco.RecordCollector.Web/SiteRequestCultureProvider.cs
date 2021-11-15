using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Krompaco.RecordCollector.Content.IO;
using Krompaco.RecordCollector.Content.Languages;
using Krompaco.RecordCollector.Web.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Krompaco.RecordCollector.Web
{
    public class SiteRequestCultureProvider : RequestCultureProvider
    {
        private readonly FileService fileService;

        private readonly ContentCultureService contentCultureService;

        public SiteRequestCultureProvider(IConfiguration config)
        {
            this.contentCultureService = new ContentCultureService();
            this.fileService = new FileService(config.GetAppSettingsContentRootPath(), config.GetAppSettingsSectionsToExcludeFromLists(), this.contentCultureService, NullLogger.Instance);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var items = httpContext.Request.Path.ToString().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (items.Length == 0)
            {
                var rootCultures = this.fileService.GetRootCultures();

                if (rootCultures.Any())
                {
                    // TODO: Check default
                    return new ProviderCultureResult(rootCultures.First().Name);
                }

                // TODO: This is where to set the default culture
                return new ProviderCultureResult("en");
            }

            var firstDirectoryInPath = items[0];

            var doesCultureExist = this.contentCultureService.DoesCultureExist(firstDirectoryInPath);

            if (!doesCultureExist)
            {
                return null;
            }

            var cultureInfo = new CultureInfo(firstDirectoryInPath);
            return new ProviderCultureResult(cultureInfo.Name);
        }
    }
}
