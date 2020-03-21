using Krompaco.RecordCollector.Content.Models;

namespace Krompaco.RecordCollector.Web.Models
{
    public interface IHasCurrentPage<TModel>
        where TModel : SinglePage
    {
        TModel CurrentPage { get; set; }
    }
}
