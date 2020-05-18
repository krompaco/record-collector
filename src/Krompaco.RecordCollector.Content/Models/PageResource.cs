using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Krompaco.RecordCollector.Content.Models
{
    public class PageResource : IRecordCollectorFile
    {
        public PageResource()
        {
            this.Level = -1;
        }

        public string Name { get; set; }

        public string Title { get; set; }

        public Dictionary<string, string> Params { get; set; }

        public string FullName { get; set; }

        public bool IsVirtual { get; set; }

        public string Section { get; set; }

        public string ClosestSectionDirectory { get; set; }

        public CultureInfo Culture { get; set; }

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

        public SinglePage ParentPage { get; set; }

        public SinglePage PreviousPage { get; set; }

        public SinglePage NextPage { get; set; }

        public SinglePage Parent { get; set; }

        public List<SinglePage> Siblings { get; set; }

        public List<SinglePage> Ancestors { get; set; }

        public List<IRecordCollectorFile> Descendants { get; set; }

        public int Level { get; set; }
    }
}
