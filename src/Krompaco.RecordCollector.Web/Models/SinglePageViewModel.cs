using System.Collections.Generic;
using Krompaco.RecordCollector.Content.Models;

namespace Krompaco.RecordCollector.Web.Models
{
    public class SinglePageViewModel : LayoutViewModel, IHasCurrentPage<SinglePage>
    {
        public SinglePageViewModel()
        {
            this.Categories = new List<CategoryItemViewModel>();
        }

        public SinglePage? CurrentPage { get; set; }

        public List<CategoryItemViewModel> Categories { get; set; }
    }
}
