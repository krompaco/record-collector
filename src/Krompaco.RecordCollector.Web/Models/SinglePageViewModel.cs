using Krompaco.RecordCollector.Content.Models;

namespace Krompaco.RecordCollector.Web.Models
{
    public class SinglePageViewModel : LayoutViewModel, IHasCurrentPage<SinglePage>
    {
        public SinglePage CurrentPage { get; set; }
    }
}
