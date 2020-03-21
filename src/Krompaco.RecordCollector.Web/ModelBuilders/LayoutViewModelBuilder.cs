using Krompaco.RecordCollector.Content.Models;
using Krompaco.RecordCollector.Web.Models;
using Markdig;

namespace Krompaco.RecordCollector.Web.ModelBuilders
{
    public class LayoutViewModelBuilder<TViewModel, TModel>
        where TViewModel : LayoutViewModel, new()
        where TModel : SinglePage
    {
        private readonly TModel currentPage;

        private readonly TViewModel vm;

        public LayoutViewModelBuilder(TModel currentPage)
        {
            this.currentPage = currentPage;
            this.vm = new TViewModel();

            if (this.vm is IHasCurrentPage<TModel> page)
            {
                page.CurrentPage = currentPage;
            }
        }

        public LayoutViewModelBuilder<TViewModel, TModel> WithMeta()
        {
            this.vm.Title = this.currentPage.Title;
            return this;
        }

        public LayoutViewModelBuilder<TViewModel, TModel> WithMarkdownPipeline()
        {
            this.vm.MarkdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            return this;
        }

        public TViewModel GetViewModel()
        {
            return this.vm;
        }
    }
}
