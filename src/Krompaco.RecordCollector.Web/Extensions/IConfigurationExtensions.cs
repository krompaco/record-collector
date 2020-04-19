using Microsoft.Extensions.Configuration;

namespace Krompaco.RecordCollector.Web.Extensions
{
    public static class IConfigurationExtensions
    {
        public static string GetAppSettingsContentRootPath(this IConfiguration config)
        {
            var path = config.GetValue<string>("AppSettings:ContentRootPath");
            return path;
        }

        public static string GetAppSettingsStaticSiteRootPath(this IConfiguration config)
        {
            var path = config.GetValue<string>("AppSettings:StaticSiteRootPath");
            return path;
        }
    }
}
