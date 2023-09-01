namespace Krompaco.RecordCollector.Content.Models
{
    public class CascadeVariables
    {
        public CascadeVariables()
        {
            this.CustomArrayProperties = new Dictionary<string, List<string>>();
            this.CustomStringProperties = new Dictionary<string, string>();
        }

        public Dictionary<string, string> CustomStringProperties { get; set; }

        public Dictionary<string, List<string>> CustomArrayProperties { get; set; }
    }
}
