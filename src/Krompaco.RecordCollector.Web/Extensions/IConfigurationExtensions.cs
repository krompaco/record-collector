using Microsoft.Extensions.Configuration;

namespace Krompaco.RecordCollector.Web.Extensions
{
    public static class IConfigurationExtensions
    {
        public static string GetAppSettingsContentRootPath(this IConfiguration config)
        {
            var contentRootPath = config.GetValue<string>("AppSettings:ContentRootPath");
            return contentRootPath;
        }
    }
}
