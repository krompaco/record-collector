using System.Globalization;

namespace Krompaco.RecordCollector.Content.Models
{
    public class FileResource : IRecordCollectorFile
    {
        public string? Name { get; set; }

        public string? Title { get; set; }

        public Uri Permalink { get; set; } = null!;

        public string? Section { get; set; }

        public string? ClosestSectionDirectory { get; set; }

        public CultureInfo Culture { get; set; } = null!;

        public Uri RelativeUrl { get; set; } = null!;

        public Uri RelativePath { get; set; } = null!;

        public bool IsSlug { get; set; }

        public bool IsFrontMatterUrl { get; set; }

        public bool IsAlias { get; set; }

        public bool IsExpanded { get; set; }

        public bool IsSelected { get; set; }

        public bool IsVisibleInNavigation { get; set; }

        public bool IsVisibleInBreadcrumbs { get; set; }

        public bool IsPage { get; set; }

        public SinglePage? ParentPage { get; set; }

        public SinglePage? PreviousPage { get; set; }

        public SinglePage? NextPage { get; set; }

        public List<SinglePage> Siblings { get; set; } = new();

        public List<SinglePage> Ancestors { get; set; } = new();

        public List<IRecordCollectorFile> Descendants { get; set; } = new();

        public int Level { get; set; } = -1;

        public Dictionary<string, string> Params { get; set; } = new();

        public string FullName { get; set; } = string.Empty;

        public bool IsVirtual { get; set; }
    }
}
