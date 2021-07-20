using System.Collections.Generic;

namespace Krompaco.RecordCollector.Content.Models
{
    public class ListPage : SinglePage
    {
        public ListPage()
        {
            this.Level = -1;
        }

        public string ListCategory { get; set; }

        public List<SinglePage> DescendantPages { get; set; }

        public List<SinglePage> CategoryPages { get; set; }
    }
}
