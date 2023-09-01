using System.Collections.Generic;
using System.Globalization;
using Krompaco.RecordCollector.Content.Models;
using Markdig;
using Microsoft.Extensions.Localization;

namespace Krompaco.RecordCollector.Web.Models
{
    public class LayoutViewModel
    {
        public string? Title { get; set; }

        public string? Robots { get; set; }

        public string? Description { get; set; }

        public string? Keywords { get; set; }

        public string? CurrentPath { get; set; }

        public List<CultureInfo>? RootCultures { get; set; }

        public CultureInfo? CurrentCulture { get; set; }

        public MarkdownPipeline? MarkdownPipeline { get; set; }

        public List<MenuItemViewModel>? NavigationItems { get; set; }

        public IStringLocalizer Localizer { get; set; } = null!;

        public ContentProperties ContentProperties { get; set; } = null!;
    }
}
