using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Krompaco.RecordCollector.Content.Models
{
    public class SinglePage : IFile
    {
        public string Type { get; set; }

        public List<string> Categories { get; set; }

        public DateTime Date { get; set; }

        public DateTime PublishDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public DateTime LastMod { get; set; }

        public string Keywords { get; set; }

        public string Description { get; set; }

        public string Slug { get; set; }

        public Uri Url { get; set; }

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

        public IFile Parent { get; set; }

        public List<SinglePage> Siblings { get; set; }

        public List<IFile> Ancestors { get; set; }

        public List<IFile> Descendants { get; set; }

        public int Level { get; set; }

        public string Layout { get; set; }

        public List<string> Tags { get; set; }

        public string Title { get; set; }

        public string LinkTitle { get; set; }

        public string Summary { get; set; }

        public string Content { get; set; }

        public ContentType ContentType { get; set; }

        public string Outputs { get; set; }

        public bool Draft { get; set; }

        public bool Headless { get; set; }

        public bool IsCjkLanguage { get; set; }

        public List<Uri> Audio { get; set; }

        public List<Uri> Videos { get; set; }

        public List<Uri> Images { get; set; }

        public List<Uri> Aliases { get; set; }

        public List<string> Series { get; set; }

        public List<PageResource> PageResources { get; set; }

        public List<FileResource> FileResources { get; set; }

        public int Weight { get; set; }

        public Dictionary<string, string> CustomStringProperties { get; set; }

        public Dictionary<string, List<string>> CustomArrayProperties { get; set; }

        public CascadeVariables Cascade { get; set; }

        public string FullName { get; set; }
    }
}
