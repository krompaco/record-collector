using System;
using System.Collections.Generic;
using System.Text;

namespace Krompaco.RecordCollector.Content.Models
{
    public class ListPage : IFile
    {
        public SinglePage Index { get; set; }

        public List<SinglePage> Children { get; set; }

        public string FullName { get; set; }
    }
}
