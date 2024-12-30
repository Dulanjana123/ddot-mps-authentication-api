namespace Model.Request.Generic
{
    public class RolePaginatedRequest : GenericSearch
    {
        public RolePaginatedFilters Filters { get; set; }
    }

    public class RolePaginatedFilters : GenericPaginatedFilter
    {
        public string? UserRoleId { get; set; }
        public string? UserRoleDescription { get; set; }
    }
}
