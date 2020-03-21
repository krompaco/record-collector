using System;
using System.Collections.Generic;

namespace Krompaco.RecordCollector.Content.Models
{
    public class PageResource : IFile
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public Dictionary<string, string> Params { get; set; }

        public string FullName { get; set; }

        public Uri RelativeUrl { get; set; }
    }
}
