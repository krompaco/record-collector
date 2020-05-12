using System;
using System.Collections.Generic;
using System.Text;

namespace Krompaco.RecordCollector.Content.Models
{
    public class ContentProperties
    {
        public string ContentRootPath { get; set; }

        public string StaticSiteRootPath { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays
        public string[] SectionsToExcludeFromLists { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        public string EnvironmentProjectWebRootPath { get; set; }

        public string SiteUrl { get; set; }
    }
}
