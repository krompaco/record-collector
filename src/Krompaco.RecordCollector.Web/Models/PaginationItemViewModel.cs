using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Krompaco.RecordCollector.Web.Models
{
    public class PaginationItemViewModel
    {
        public bool IsSelected { get; set; }

        public bool IsFirst { get; set; }

        public bool IsLast { get; set; }

        public bool IsNext { get; set; }

        public bool IsPrevious { get; set; }

        public bool IsEllipsis { get; set; }

        public bool IsSeparator { get; set; }

        public int Page { get; set; }

        public Uri RelativeUrl { get; set; }
    }
}
