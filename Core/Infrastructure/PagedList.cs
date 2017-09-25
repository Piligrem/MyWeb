﻿using System;
using System.Collections;
using System.Linq;

namespace InSearch.Core
{
    public abstract class PagedListBase : IPageable
    {

        protected PagedListBase()
        {
            this.PageIndex = 0;
            this.PageSize = 0;
            this.TotalCount = 1;
        }

        protected PagedListBase(IPageable pageable)
        {
            this.Init(pageable);
        }

        protected PagedListBase(IEnumerable source, int pageIndex, int pageSize)
        {
            Guard.ArgumentNotNull(source, "source");
            Guard.PagingArgsValid(pageIndex, pageSize, "pageIndex", "pageSize");

            this.PageIndex = pageIndex;
            this.PageSize = pageSize;
            this.TotalCount = source.GetCount();
        }

        protected PagedListBase(int pageIndex, int pageSize, int totalItemsCount)
        {
            Guard.PagingArgsValid(pageIndex, pageSize, "pageIndex", "pageSize");

            this.PageIndex = pageIndex;
            this.PageSize = pageSize;
            this.TotalCount = totalItemsCount;
        }

        // only here for compatible reasons with nc
        public void LoadPagedList<T>(IPagedList<T> pagedList)
        {
            this.Init(pagedList as IPageable);
        }

        public virtual void Init(IPageable pageable)
        {
            Guard.ArgumentNotNull(pageable, "pageable");

            this.PageIndex = pageable.PageIndex;
            this.PageSize = pageable.PageSize;
            this.TotalCount = pageable.TotalCount;
        }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int PageNumber
        {
            get { return this.PageIndex + 1; }
            set { this.PageIndex = value - 1; }
        }

        public int TotalPages
        {
            get
            {
                var total = this.TotalCount / this.PageSize;

                if (this.TotalCount % this.PageSize > 0)
                    total++;

                return total;
            }
        }

        public bool HasPreviousPage
        {
            get { return this.PageIndex > 0; }
        }

        public bool HasNextPage
        {
            get { return (this.PageIndex < (this.TotalPages - 1)); }
        }

        public int FirstItemIndex
        {
            get { return (this.PageIndex * this.PageSize) + 1; }
        }

        public int LastItemIndex
        {
            get { return Math.Min(this.TotalCount, ((this.PageIndex * this.PageSize) + this.PageSize)); }
        }

        public bool IsFirstPage
        {
            get { return (this.PageIndex <= 0); }
        }

        public bool IsLastPage
        {
            get { return (this.PageIndex >= (this.TotalPages - 1)); }
        }

        public virtual IEnumerator GetEnumerator()
        {
            return Enumerable.Empty<int>().GetEnumerator();
        }
    }

    public class PagedList : PagedListBase
    {
        public PagedList(int pageIndex, int pageSize, int totalItemsCount) : base(pageIndex, pageSize, totalItemsCount) { }
    }
}
