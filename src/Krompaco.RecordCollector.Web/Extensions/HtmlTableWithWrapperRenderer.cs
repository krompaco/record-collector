using Markdig.Extensions.Tables;
using Markdig.Renderers;

namespace Krompaco.RecordCollector.Web.Extensions
{
    public class HtmlTableWithWrapperRenderer : Markdig.Extensions.Tables.HtmlTableRenderer
    {
        protected override void Write(HtmlRenderer renderer, Table table)
        {
            renderer.EnsureLine();
            renderer.WriteLine("<div class=\"table-wrapper\">");
            base.Write(renderer, table);
            renderer.WriteLine("</div>");
        }
    }
}
