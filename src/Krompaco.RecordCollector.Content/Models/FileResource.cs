using System;
using System.Collections.Generic;
using System.Text;

namespace Krompaco.RecordCollector.Content.Models
{
    public class FileResource : IFile
    {
        public string Name { get; set; }

        public Uri Permalink { get; set; }

        public string FullName { get; set; }
    }
}
