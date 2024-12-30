namespace Model.Request.Generic
{
    public class PagingAndSortingInfo
    {
        public PagingInfo Paging { get; set; }
        //public SortingInfoCollection Sorting { get; set; } //Todo

        public PagingAndSortingInfo()
        {
            this.Paging = new PagingInfo();
            //this.Sorting = new SortingInfoCollection(); //Todo
        }

        public PagingAndSortingInfo MapToObject()
        {
            PagingAndSortingInfo pagingAndSortingInfo = new PagingAndSortingInfo();

            pagingAndSortingInfo.Paging.PageNo = this.Paging.PageNo;
            pagingAndSortingInfo.Paging.PageSize = this.Paging.PageSize;

            //foreach (SortingInfo sortingInfo in this.Sorting) //Todo
            //{
            //    pagingAndSortingInfo.Sorting.Add(new QuerySortingInfo()
            //    {
            //        ColumnName = sortingInfo.ColumnName,
            //        SortOrder = sortingInfo.SortOrder
            //    });
            //}

            return pagingAndSortingInfo;
        }
    }

    public class PagingInfo
    {
        public int PageNo { get; set; }
        public int PageSize { get; set; }
    }

    public class SortingInfo
    {
        public string ColumnName { get; set; }
        public string SortOrder { get; set; }
    }

    public class SortingInfoCollection : List<SortingInfo>
    {
    }
}
