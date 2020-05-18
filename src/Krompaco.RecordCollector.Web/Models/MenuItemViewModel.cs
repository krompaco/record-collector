using System;
using System.Collections.Generic;

namespace Krompaco.RecordCollector.Web.Models
{
    public class MenuItemViewModel
    {
        public bool IsSelected { get; set; }

        public bool HasChildren { get; set; }

        public List<MenuItemViewModel> ChildItems { get; set; }

        public Uri RelativeUrl { get; set; }

        public string Text { get; set; }

        public int Level { get; set; }
    }
}
