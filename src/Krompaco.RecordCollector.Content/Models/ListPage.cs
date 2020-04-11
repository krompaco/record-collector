using System.Collections.Generic;

namespace Krompaco.RecordCollector.Content.Models
{
    public class ListPage : SinglePage
    {
        public List<SinglePage> DescendantPages { get; set; }
    }
}
