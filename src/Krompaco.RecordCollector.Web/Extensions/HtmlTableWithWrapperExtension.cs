using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Parsers;
using Markdig.Renderers;

namespace Krompaco.RecordCollector.Web.Extensions
{
    public class HtmlTableWithWrapperExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (!(renderer is HtmlRenderer htmlRenderer) || htmlRenderer.ObjectRenderers.Contains<HtmlTableRenderer>())
            {
                return;
            }

            htmlRenderer.ObjectRenderers.Add(new HtmlTableWithWrapperRenderer());
        }
    }
}
