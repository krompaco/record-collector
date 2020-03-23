using System;
using System.Collections.Generic;
using System.Globalization;
using Markdig;

namespace Krompaco.RecordCollector.Web.Models
{
    public class LayoutViewModel
    {
        public string Title { get; set; }

        public string CurrentPath { get; set; }

        public List<CultureInfo> RootCultures { get; set; }

        public CultureInfo CurrentCulture { get; set; }

        public MarkdownPipeline MarkdownPipeline { get; set; }
    }
}
