using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        public LayoutViewModelBuilder(TModel currentPage, CultureInfo currentCulture, List<CultureInfo> rootCultures, HttpRequest request)
        {
            this.currentPage = currentPage;
            this.vm = new TViewModel();

            if (this.vm is IHasCurrentPage<TModel> vmWithCurrentPageProperty)
            {
                vmWithCurrentPageProperty.CurrentPage = currentPage;
            }

            this.vm.RootCultures = rootCultures ?? new List<CultureInfo>();
            this.vm.CurrentCulture = currentCulture;
            this.vm.NavigationItems = new List<MenuItemViewModel>();
            this.vm.CurrentPath = !string.IsNullOrEmpty(request?.Path.Value) ? request.Path.Value : "/";

            if (this.vm is ListPageViewModel listPageViewModel)
            {
                listPageViewModel.PaginationItems = new List<PaginationItemViewModel>();
                listPageViewModel.PagedDescendantPages = new List<SinglePage>();
            }
        }

        public LayoutViewModelBuilder<TViewModel, TModel> WithMeta()
        {
            this.vm.Title = this.currentPage.Title;
            return this;
        }

        public LayoutViewModelBuilder<TViewModel, TModel> WithPaginationItems(int pageCount, int pageSize)
        {
            if (!(this.vm is ListPageViewModel listPageViewModel)
                || !(this.currentPage is ListPage listPage))
            {
                return this;
            }

            var builder = new PaginationViewModelBuilder(
                this.vm.CurrentPath,
                listPage.DescendantPages.Count,
                pageCount,
                pageSize);

            listPageViewModel.PagedDescendantPages = listPage.DescendantPages.Skip(pageSize * (builder.SelectedPage - 1)).Take(pageSize).ToList();

            // This updates builder.SelectedPage to the highest available page
            listPageViewModel.PaginationItems = builder.GetPaginationItems().ToList();

            return this;
        }

        public LayoutViewModelBuilder<TViewModel, TModel> WithNavigationItems(List<SinglePage> pages)
        {
            if (pages == null)
            {
                throw new ArgumentNullException(nameof(pages));
            }

            foreach (var page in pages)
            {
                var item = new MenuItemViewModel();

                if (this.vm.CurrentPath.Equals(page.RelativeUrl.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    item.IsSelected = true;
                }

                item.ChildItems = new List<MenuItemViewModel>();
                item.HasChildren = item.ChildItems.Any();
                item.Level = 0;
                item.Text = string.IsNullOrWhiteSpace(page.LinkTitle) ? page.Title : page.LinkTitle;
                item.RelativeUrl = page.RelativeUrl;

                this.vm.NavigationItems.Add(item);
            }

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
