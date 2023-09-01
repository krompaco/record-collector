using System.Globalization;

namespace Krompaco.RecordCollector.Content.Models
{
    public class SinglePage : IRecordCollectorFile
    {
        public string? Type { get; set; }

        public List<string> Categories { get; set; } = new();

        public DateTime Date { get; set; }

        public DateTime PublishDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public DateTime LastMod { get; set; }

        public string? Keywords { get; set; }

        public string? Description { get; set; }

        public string? Slug { get; set; }

        public Uri Url { get; set; } = null!;

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

        public string? Layout { get; set; }

        public List<string> Tags { get; set; } = new();

        public string? Title { get; set; }

        public string? LinkTitle { get; set; }

        public string? Summary { get; set; }

        public string? Content { get; set; }

        public ContentType ContentType { get; set; }

        public string? Outputs { get; set; }

        public bool Draft { get; set; }

        public bool Headless { get; set; }

        public bool IsCjkLanguage { get; set; }

        public List<Uri> Audio { get; set; } = new();

        public List<Uri> Videos { get; set; } = new();

        public List<Uri> Images { get; set; } = new();

        public List<Uri> Aliases { get; set; } = new();

        public List<string> Series { get; set; } = new();

        public List<PageResource> PageResources { get; set; } = new();

        public List<FileResource> FileResources { get; set; } = new();

        public int Weight { get; set; }

        public Dictionary<string, string> CustomStringProperties { get; set; } = new();

        public Dictionary<string, List<string>> CustomArrayProperties { get; set; } = new();

        public CascadeVariables? Cascade { get; set; }

        public string FullName { get; set; } = string.Empty;

        public bool IsVirtual { get; set; }
    }
}
