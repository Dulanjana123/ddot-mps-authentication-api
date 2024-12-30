using Model.Request.Generic;

namespace Model.Request
{
    public class AgencyPaginatedRequest: GenericSearch
    {
        public AgencyPaginatedFilters Filters { get; set; }
    }

    public class AgencyPaginatedFilters: GenericPaginatedFilter
    {
        public string AgencyCode { get; set; } = string.Empty;
        public string AgencyName { get; set; } = string.Empty;
    }
}
