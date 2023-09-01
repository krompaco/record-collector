namespace Krompaco.RecordCollector.Content.Models
{
    public class ContentProperties
    {
        public ContentProperties()
        {
            this.ContentRootPath = string.Empty;
            this.EnvironmentProjectWebRootPath = string.Empty;
            this.StaticSiteRootPath = string.Empty;
            this.SectionsToExcludeFromLists = new List<string>();
            this.SiteUrl = string.Empty;
            this.FrontendSetup = string.Empty;
        }

        public string ContentRootPath { get; set; }

        public string StaticSiteRootPath { get; set; }

        public List<string> SectionsToExcludeFromLists { get; set; }

        public string EnvironmentProjectWebRootPath { get; set; }

#pragma warning disable CA1056 // URI-like properties should not be strings
        public string SiteUrl { get; set; }
#pragma warning restore CA1056 // URI-like properties should not be strings

        public string FrontendSetup { get; set; }
    }
}
