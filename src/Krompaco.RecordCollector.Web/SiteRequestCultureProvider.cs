using System.Globalization;
using Krompaco.RecordCollector.Content.IO;
using Krompaco.RecordCollector.Content.Languages;
using Krompaco.RecordCollector.Web.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Krompaco.RecordCollector.Web;

public class SiteRequestCultureProvider : RequestCultureProvider
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    // ReSharper disable once AsyncMethodWithoutAwait
    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        var monitor = httpContext.RequestServices.GetService<IOptionsMonitor<AppSettings>>();
        var appSettings = monitor!.CurrentValue;

        var contentCultureService = new ContentCultureService();
        var fileService = new FileService(appSettings.ContentRootPath, appSettings.SectionsToExcludeFromLists.ToList(), contentCultureService, NullLogger.Instance);

        var items = httpContext.Request.Path.ToString().Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        if (items.Length == 0)
        {
            var rootCultures = fileService.GetRootCultures();

            if (rootCultures.Any())
            {
                // TODO: Check default
                return new ProviderCultureResult(rootCultures.First().Name);
            }

            // TODO: This is where to set the default culture
            return new ProviderCultureResult("en");
        }

        var firstDirectoryInPath = items[0];

        var doesCultureExist = contentCultureService.DoesCultureExist(firstDirectoryInPath);

        if (!doesCultureExist)
        {
            return null;
        }

        var cultureInfo = new CultureInfo(firstDirectoryInPath);
        return new ProviderCultureResult(cultureInfo.Name);
    }
}
