namespace Krompaco.RecordCollector.Web.Extensions
{
    // TODO: Convert to config model
    public static class IConfigurationExtensions
    {
#pragma warning disable CA1055 // Uri return values should not be strings
        public static string GetAppSettingsSiteUrl(this IConfiguration config)
#pragma warning restore CA1055 // Uri return values should not be strings
        {
            var path = config.GetValue<string>("AppSettings:SiteUrl");
            return path;
        }

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

        public static string GetAppSettingsFrontendSetup(this IConfiguration config)
        {
            var setup = config.GetValue<string>("AppSettings:FrontendSetup");
            return setup;
        }

        public static string GetAppSettingsViewPrefix(this IConfiguration config)
        {
            var viewPrefix = config.GetValue<string>("AppSettings:ViewPrefix");
            return viewPrefix;
        }

#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
        public static List<string> GetAppSettingsMainNavigationSections(this IConfiguration config)
        {
            var sections = config?.GetSection("AppSettings:MainNavigationSections").Get<string[]>();
            return sections?.ToList() ?? new List<string>();
        }

        public static List<string> GetAppSettingsSectionsToExcludeFromLists(this IConfiguration config)
        {
            var sections = config?.GetSection("AppSettings:SectionsToExcludeFromLists").Get<string[]>();
            return sections?.ToList() ?? new List<string>();
        }
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly

        public static int GetAppSettingsPaginationPageCount(this IConfiguration config)
        {
            var value = config.GetValue<int>("AppSettings:PaginationPageCount");
            return value;
        }

        public static int GetAppSettingsPaginationPageSize(this IConfiguration config)
        {
            var value = config.GetValue<int>("AppSettings:PaginationPageSize");
            return value;
        }
    }
}
