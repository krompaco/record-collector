namespace Krompaco.RecordCollector.Content.Models
{
    public class ContentProperties
    {
        public string ContentRootPath { get; set; }

        public string StaticSiteRootPath { get; set; }

        public List<string> SectionsToExcludeFromLists { get; set; }

        public string EnvironmentProjectWebRootPath { get; set; }

        public string SiteUrl { get; set; }

        public string FrontendSetup { get; set; }
    }
}
