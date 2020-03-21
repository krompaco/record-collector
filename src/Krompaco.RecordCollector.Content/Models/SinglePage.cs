using System;
using System.Collections.Generic;
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

        public Uri RelativeUrl { get; set; }

        public string Layout { get; set; }

        public List<string> Tags { get; set; }

        public string Title { get; set; }

        public string LinkTitle { get; set; }

        public string Summary { get; set; }

        public TextReader ContentTextReader { get; set; }

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
