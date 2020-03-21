using Markdig;

namespace Krompaco.RecordCollector.Web.Models
{
    public class LayoutViewModel
    {
        public string Title { get; set; }

        public MarkdownPipeline MarkdownPipeline { get; set; }
    }
}
