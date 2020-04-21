using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Krompaco.RecordCollector.Web.Models;

namespace Krompaco.RecordCollector.Web.ModelBuilders
{
    public class PaginationViewModelBuilder
    {
        private readonly Uri currentUrl;

        private readonly int totalCount;

        private int pageCount;

        private int pageSize;

        private int selectedPage;

        public PaginationViewModelBuilder(
            Uri currentUrl,
            int totalCount,
            int pageCount,
            int pageSize,
            int selectedPage)
        {
            this.currentUrl = currentUrl;
            this.totalCount = totalCount;
            this.pageCount = pageCount;
            this.pageSize = pageSize;
            this.selectedPage = selectedPage;
        }

        public IEnumerable<PaginationItemViewModel> GetPaginationItems()
        {
            if (this.totalCount < 1)
            {
                yield break;
            }

            if (this.pageCount < 1)
            {
                this.pageCount = int.MaxValue;
            }

            if (this.pageSize < 1)
            {
                this.pageSize = 1;
            }

            var totalPages = (int)Math.Ceiling((double)this.totalCount / (double)this.pageSize);

            if (this.selectedPage < 1)
            {
                this.selectedPage = 1;
            }

            if (this.selectedPage > totalPages)
            {
                this.selectedPage = totalPages;
            }

            var start = 1;
            var end = this.pageCount;

            if (totalPages > this.pageCount)
            {
                if (this.selectedPage > (this.pageCount / 2))
                {
                    start = this.selectedPage - (this.pageCount / 2);
                    end = start + this.pageCount - 1;
                }

                if (this.selectedPage > totalPages - (this.pageCount / 2))
                {
                    end = totalPages;
                    start = end - this.pageCount + 1;
                }
            }

            if (end > totalPages)
            {
                end = totalPages;
            }

            if (this.selectedPage > start)
            {
                yield return new PaginationItemViewModel
                {
                    Page = this.selectedPage - 1,
                    RelativeUrl = this.GetUrl(this.selectedPage - 1),
                    IsPrevious = true,
                };

                yield return this.Separator(this.selectedPage);

                if (start > 1)
                {
                    yield return new PaginationItemViewModel
                    {
                        Page = 1,
                        RelativeUrl = this.GetUrl(1),
                        IsFirst = true,
                    };

                    yield return this.Separator(this.selectedPage);
                }

                if (start > 2)
                {
                    yield return new PaginationItemViewModel
                    {
                        Page = this.selectedPage - 1,
                        RelativeUrl = this.GetUrl(this.selectedPage - 1),
                        IsEllipsis = true,
                    };

                    yield return this.Separator(this.selectedPage);
                }
            }

            for (var i = start; i <= end; i++)
            {
                yield return new PaginationItemViewModel
                {
                    Page = i,
                    RelativeUrl = this.GetUrl(i),
                    IsSelected = i == this.selectedPage,
                };

                if (i < end)
                {
                    yield return this.Separator(this.selectedPage);
                }
            }

            if (this.selectedPage >= totalPages)
            {
                yield break;
            }

            if (end < (totalPages - 1))
            {
                yield return this.Separator(this.selectedPage);

                yield return new PaginationItemViewModel
                {
                    Page = this.selectedPage + 1,
                    RelativeUrl = this.GetUrl(this.selectedPage + 1),
                    IsEllipsis = true,
                };
            }

            if (end < totalPages)
            {
                yield return this.Separator(this.selectedPage);

                yield return new PaginationItemViewModel
                {
                    Page = totalPages,
                    RelativeUrl = this.GetUrl(totalPages),
                    IsLast = true,
                };
            }

            yield return this.Separator(this.selectedPage);

            yield return new PaginationItemViewModel
            {
                Page = this.selectedPage + 1,
                RelativeUrl = this.GetUrl(this.selectedPage + 1),
                IsNext = true,
            };
        }

        private PaginationItemViewModel Separator(int page)
        {
            return new PaginationItemViewModel
            {
                Page = page,
                RelativeUrl = this.GetUrl(page),
                IsSeparator = true,
            };
        }

        private Uri GetUrl(int page)
        {
            var current = this.currentUrl.ToString();
            current = Regex.Replace(current, "/page-\\d+/", "/", RegexOptions.IgnoreCase);
            current = current.TrimEnd('/');
            var newUrl = $"{current}/page-{page}/";
            return new Uri(newUrl, UriKind.Relative);
        }
    }
}
