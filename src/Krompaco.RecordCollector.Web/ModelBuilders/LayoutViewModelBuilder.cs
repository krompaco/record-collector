using System.Globalization;
using Krompaco.RecordCollector.Content.Models;
using Krompaco.RecordCollector.Web.Extensions;
using Krompaco.RecordCollector.Web.Models;
using Markdig;
using Microsoft.Extensions.Localization;

namespace Krompaco.RecordCollector.Web.ModelBuilders
{
    public class LayoutViewModelBuilder<TViewModel, TModel>
        where TViewModel : LayoutViewModel, new()
        where TModel : SinglePage
    {
        private readonly TModel currentPage;

        private readonly TViewModel vm;

        public LayoutViewModelBuilder(TModel currentPage, CultureInfo currentCulture, List<CultureInfo> rootCultures, HttpRequest request, IStringLocalizer localizer, ContentProperties contentProperties)
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
            this.vm.Localizer = localizer;
            this.vm.ContentProperties = contentProperties;

            if (!(this.vm is ListPageViewModel listPageViewModel))
            {
                return;
            }

            listPageViewModel.Pagination =
                new PaginationViewModel
                {
                    Layout = this.vm,
                    Items = new List<PaginationItemViewModel>()
                };
            listPageViewModel.PagedDescendantPages = new List<SinglePage>();
        }

        public LayoutViewModelBuilder<TViewModel, TModel> WithMeta()
        {
            this.vm.Title = this.currentPage.Title;
            this.vm.Description = this.currentPage.Description;
            this.vm.Keywords = this.currentPage.Keywords;

            const string RobotsKey = "robots";

            if (this.currentPage.CustomStringProperties.ContainsKey(RobotsKey))
            {
                this.vm.Robots = this.currentPage.CustomStringProperties[RobotsKey];
            }

            return this;
        }

        public LayoutViewModelBuilder<TViewModel, TModel> WithPaginationItems(int pageCount, int pageSize)
        {
            if (this.vm is not ListPageViewModel listPageViewModel
                || this.currentPage is not ListPage listPage)
            {
                return this;
            }

            var pageToList = string.IsNullOrWhiteSpace(listPage.ListCategory) ? listPage.DescendantPages : listPage.CategoryPages;

            var builder = new PaginationViewModelBuilder(
                this.vm.CurrentPath,
                pageToList.Count,
                pageCount,
                pageSize);

            listPageViewModel.PagedDescendantPages = pageToList.Skip(pageSize * (builder.SelectedPage - 1)).Take(pageSize).ToList();

            // This updates builder.SelectedPage to the highest available page
            listPageViewModel.Pagination!.Items = builder.GetPaginationItems().ToList();

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
                this.vm.NavigationItems?.Add(this.GetMenuItemViewModelFromPage(page));
            }

            return this;
        }

        public LayoutViewModelBuilder<TViewModel, TModel> WithMarkdownPipeline()
        {
            this.vm.MarkdownPipeline = new MarkdownPipelineBuilder()
                .Use<HtmlTableWithWrapperExtension>()
                .UseAbbreviations()
                .UseCitations()
                .UseDefinitionLists()
                .UseEmphasisExtras()
                .UseFigures()
                .UseFooters()
                .UseFootnotes()
                .UseGridTables()
                .UseMathematics()
                .UseMediaLinks()
                .UsePipeTables()
                .UseListExtras()
                .UseTaskLists()
                .UseDiagrams()
                .UseAutoLinks()
                .UseGenericAttributes()
                .Build();

            return this;
        }

        public TViewModel GetViewModel()
        {
            return this.vm;
        }

        private MenuItemViewModel GetMenuItemViewModelFromPage(SinglePage page)
        {
            var item = new MenuItemViewModel();

            if (this.vm.CurrentPath?.Equals(page.RelativeUrl.ToString(), StringComparison.OrdinalIgnoreCase) ?? false)
            {
                item.IsSelected = true;
            }

            item.ChildItems = new List<MenuItemViewModel>();

            if (page.Descendants != null && page.Descendants.Any(x => x.Level == page.Level + 1))
            {
                var childPages = page.Descendants
                    .Where(x => x.Level == page.Level + 1
                                && x.GetType() == typeof(SinglePage))
                    .Select(x => (SinglePage)x)
                    .OrderBy(x => x.Weight)
                    .ToList();

                foreach (var childPage in childPages)
                {
                    item.ChildItems.Add(this.GetMenuItemViewModelFromPage(childPage));
                }
            }

            item.HasChildren = item.ChildItems.Any();
            item.Level = page.Level;
            item.Text = string.IsNullOrWhiteSpace(page.LinkTitle) ? page.Title : page.LinkTitle;
            item.RelativeUrl = page.RelativeUrl;
            item.Localizer = this.vm.Localizer;

            return item;
        }
    }
}
