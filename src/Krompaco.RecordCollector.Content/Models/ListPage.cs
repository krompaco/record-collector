namespace Krompaco.RecordCollector.Content.Models
{
    public class ListPage : SinglePage
    {
        public ListPage()
        {
            this.Level = -1;
            this.DescendantPages = new List<SinglePage>();
            this.CategoryPages = new List<SinglePage>();
        }

        public string? ListCategory { get; set; }

        public List<SinglePage> DescendantPages { get; set; }

        public List<SinglePage> CategoryPages { get; set; }
    }
}
