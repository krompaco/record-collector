using System;

namespace Krompaco.RecordCollector.Web.Models
{
    public class CategoryItemViewModel
    {
        public CategoryItemViewModel()
        {
            this.Text = string.Empty;
        }

        public Uri? RelativeUrl { get; set; }

        public string Text { get; set; }

        public int PageCount { get; set; }
    }
}
