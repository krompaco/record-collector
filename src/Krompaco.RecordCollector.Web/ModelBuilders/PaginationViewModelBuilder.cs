using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Krompaco.RecordCollector.Web.Models;

namespace Krompaco.RecordCollector.Web.ModelBuilders
{
    public class PaginationViewModelBuilder
    {
        private readonly string currentPath;

        private readonly int totalCount;

        private int pageCount;

        private int pageSize;

        public PaginationViewModelBuilder(
            string? currentPath,
            int totalCount,
            int pageCount,
            int pageSize)
        {
            this.currentPath = currentPath ?? string.Empty;
            this.totalCount = totalCount;
            this.pageCount = pageCount;
            this.pageSize = pageSize;
            this.SelectedPage = this.GetCurrentPageNumber();
        }

        public int SelectedPage { get; set; }

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

            if (this.SelectedPage < 1)
            {
                this.SelectedPage = 1;
            }

            if (this.SelectedPage > totalPages)
            {
                this.SelectedPage = totalPages;
            }

            var start = 1;
            var end = this.pageCount;

            if (totalPages > this.pageCount)
            {
                if (this.SelectedPage > (this.pageCount / 2))
                {
                    start = this.SelectedPage - (this.pageCount / 2);
                    end = start + this.pageCount - 1;
                }

                if (this.SelectedPage > totalPages - (this.pageCount / 2))
                {
                    end = totalPages;
                    start = end - this.pageCount + 1;
                }
            }

            if (end > totalPages)
            {
                end = totalPages;
            }

            if (this.SelectedPage > start)
            {
                yield return new PaginationItemViewModel
                {
                    Page = this.SelectedPage - 1,
                    RelativeUrl = this.GetUrl(this.SelectedPage - 1),
                    IsPrevious = true,
                };

                yield return this.Separator(this.SelectedPage);

                if (start > 1)
                {
                    yield return new PaginationItemViewModel
                    {
                        Page = 1,
                        RelativeUrl = this.GetUrl(1),
                        IsFirst = true,
                    };

                    yield return this.Separator(this.SelectedPage);
                }

                if (start > 2)
                {
                    yield return new PaginationItemViewModel
                    {
                        Page = this.SelectedPage - 1,
                        RelativeUrl = this.GetUrl(this.SelectedPage - 1),
                        IsEllipsis = true,
                    };

                    yield return this.Separator(this.SelectedPage);
                }
            }

            for (var i = start; i <= end; i++)
            {
                yield return new PaginationItemViewModel
                {
                    Page = i,
                    RelativeUrl = this.GetUrl(i),
                    IsSelected = i == this.SelectedPage,
                };

                if (i < end)
                {
                    yield return this.Separator(this.SelectedPage);
                }
            }

            if (this.SelectedPage >= totalPages)
            {
                yield break;
            }

            if (end < totalPages - 1)
            {
                yield return this.Separator(this.SelectedPage);

                yield return new PaginationItemViewModel
                {
                    Page = this.SelectedPage + 1,
                    RelativeUrl = this.GetUrl(this.SelectedPage + 1),
                    IsEllipsis = true,
                };
            }

            if (end < totalPages)
            {
                yield return this.Separator(this.SelectedPage);

                yield return new PaginationItemViewModel
                {
                    Page = totalPages,
                    RelativeUrl = this.GetUrl(totalPages),
                    IsLast = true,
                };
            }

            yield return this.Separator(this.SelectedPage);

            yield return new PaginationItemViewModel
            {
                Page = this.SelectedPage + 1,
                RelativeUrl = this.GetUrl(this.SelectedPage + 1),
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
            var current = this.currentPath;
            current = Regex.Replace(current, "/page-\\d+/$", "/", RegexOptions.IgnoreCase);
            current = current.TrimEnd('/');
            var newUrl = page == 1 ? $"{current}/" : $"{current}/page-{page}/";
            return new Uri(newUrl, UriKind.Relative);
        }

        private int GetCurrentPageNumber()
        {
            var match = Regex.Match(this.currentPath, "/page-(\\d+)/$", RegexOptions.IgnoreCase);

            if (match.Success && match.Groups.Count > 1)
            {
                var matchValue = match.Groups[1].Value;
                return Convert.ToInt32(matchValue, CultureInfo.InvariantCulture);
            }

            return 1;
        }
    }
}
