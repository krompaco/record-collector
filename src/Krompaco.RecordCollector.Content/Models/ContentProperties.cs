using System;
using System.Collections.Generic;
using System.Text;

namespace Krompaco.RecordCollector.Content.Models
{
    public class ContentProperties
    {
        public string ContentRootPath { get; set; }

        public string StaticSiteRootPath { get; set; }

        public string[] SectionsToExcludeFromLists { get; set; }

        public string EnvironmentProjectWebRootPath { get; set; }
    }
}
