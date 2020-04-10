using System;
using System.Collections.Generic;
using System.Globalization;

namespace Krompaco.RecordCollector.Content.Models
{
    public interface IFile
    {
        string FullName { get; set; }

        string Section { get; set; }

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

        IFile Parent { get; set; }

        List<IFile> Children { get; set; }

        List<IFile> Ancestors { get; set; }

        List<IFile> Descendants { get; set; }

        int Level { get; set; }
    }
}
