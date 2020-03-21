using Krompaco.RecordCollector.Content.Models;

namespace Krompaco.RecordCollector.Web.Models
{
    public class ListPageViewModel : LayoutViewModel, IHasCurrentPage<ListPage>
    {
        public ListPage CurrentPage { get; set; }
    }
}
