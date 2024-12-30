namespace Model.Request.Generic
{
    public abstract class GenericSearch
    {
        public PagingAndSortingInfo PagingAndSortingInfo { get; set; }

        public GenericSearch()
        {
            PagingAndSortingInfo = new PagingAndSortingInfo();
        }
    }
}
