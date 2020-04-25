using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Krompaco.RecordCollector.Web.Models
{
    public class PaginationViewModel
    {
        public LayoutViewModel Layout { get; set; }

        public List<PaginationItemViewModel> Items { get; set; }
    }
}
