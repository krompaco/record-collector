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

        public static string GetAppSettingsMainNavigationSection(this IConfiguration config)
        {
            var path = config.GetValue<string>("AppSettings:MainNavigationSection");
            return path;
        }

        public static string[] GetAppSettingsSectionsToExcludeFromLists(this IConfiguration config)
        {
            var sections = config?.GetSection("AppSettings:SectionsToExcludeFromLists").Get<string[]>();
            return sections;
        }
    }
}
