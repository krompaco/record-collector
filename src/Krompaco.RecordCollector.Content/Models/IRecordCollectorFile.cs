using System.Globalization;

namespace Krompaco.RecordCollector.Content.Models
{
    public interface IRecordCollectorFile
    {
        string FullName { get; set; }

        bool IsVirtual { get; set; }

        string? Title { get; set; }

        string? Section { get; set; }

        string? ClosestSectionDirectory { get; set; }

        CultureInfo Culture { get; set; }

        Uri RelativeUrl { get; set; }

        Uri RelativePath { get; set; }

        bool IsSlug { get; set; }

        bool IsFrontMatterUrl { get; set; }

        bool IsAlias { get; set; }

        bool IsExpanded { get; set; }

        bool IsSelected { get; set; }

        bool IsVisibleInNavigation { get; set; }

        bool IsVisibleInBreadcrumbs { get; set; }

        bool IsPage { get; set; }

        SinglePage? ParentPage { get; set; }

        SinglePage? PreviousPage { get; set; }

        SinglePage? NextPage { get; set; }

        List<SinglePage> Siblings { get; set; }

        List<SinglePage> Ancestors { get; set; }

        List<IRecordCollectorFile> Descendants { get; set; }

        int Level { get; set; }
    }
}
