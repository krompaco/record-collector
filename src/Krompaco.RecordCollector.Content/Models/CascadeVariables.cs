using System.Collections.Generic;

namespace Krompaco.RecordCollector.Content.Models
{
    public class CascadeVariables
    {
        public Dictionary<string, string> CustomStringProperties { get; set; }

        public Dictionary<string, List<string>> CustomArrayProperties { get; set; }
    }
}
