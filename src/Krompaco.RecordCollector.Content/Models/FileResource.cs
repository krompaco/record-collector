using System;
using System.Collections.Generic;

namespace Krompaco.RecordCollector.Content.Models
{
    public class FileResource : IFile
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public Uri Permalink { get; set; }

        public string Section { get; set; }

        public Uri RelativeUrl { get; set; }

        public Uri RelativePath { get; set; }

        public bool IsSlug { get; set; }

        public bool IsFrontMatterUrl { get; set; }

        public bool IsAlias { get; set; }

        public bool IsExpanded { get; set; }

        public bool IsSelected { get; set; }

        public bool IsVisibleInNavigation { get; set; }

        public bool IsVisibleInBreadcrumbs { get; set; }

        public bool IsPage { get; set; }

        public IFile Parent { get; set; }

        public List<IFile> Children { get; set; }

        public List<IFile> Ancestors { get; set; }

        public List<IFile> Descendants { get; set; }

        public int Level { get; set; }

        public Dictionary<string, string> Params { get; set; }

        public string FullName { get; set; }
    }
}
