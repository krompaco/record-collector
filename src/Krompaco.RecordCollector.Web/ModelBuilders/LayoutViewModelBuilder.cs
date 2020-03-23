using System;
using System.Collections.Generic;
using System.Globalization;
using Krompaco.RecordCollector.Content.IO;
using Krompaco.RecordCollector.Content.Models;
using Krompaco.RecordCollector.Web.Models;
using Markdig;
using Microsoft.AspNetCore.Http;

namespace Krompaco.RecordCollector.Web.ModelBuilders
{
    public class LayoutViewModelBuilder<TViewModel, TModel>
        where TViewModel : LayoutViewModel, new()
        where TModel : SinglePage
    {
        private readonly TModel currentPage;

        private readonly TViewModel vm;

        public LayoutViewModelBuilder(TModel currentPage, CultureInfo currentCulture, List<CultureInfo> rootCultures)
        {
            this.currentPage = currentPage;
            this.vm = new TViewModel();

            if (this.vm is IHasCurrentPage<TModel> vmWithCurrentPageProperty)
            {
                vmWithCurrentPageProperty.CurrentPage = currentPage;
            }

            this.vm.RootCultures = rootCultures ?? new List<CultureInfo>();
            this.vm.CurrentCulture = currentCulture;
        }

        public LayoutViewModelBuilder<TViewModel, TModel> WithMeta(HttpRequest request)
        {
            this.vm.Title = this.currentPage.Title;
            this.vm.CurrentPath = !string.IsNullOrEmpty(request?.Path.Value) ? request.Path.Value : "/";
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
