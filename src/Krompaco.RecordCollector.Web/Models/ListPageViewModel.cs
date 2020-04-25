using System.Collections.Generic;
using Krompaco.RecordCollector.Content.Models;

namespace Krompaco.RecordCollector.Web.Models
{
    public class ListPageViewModel : LayoutViewModel, IHasCurrentPage<ListPage>
    {
        public ListPage CurrentPage { get; set; }

        public PaginationViewModel Pagination { get; set; }

        public List<SinglePage> PagedDescendantPages { get; set; }
    }
}
