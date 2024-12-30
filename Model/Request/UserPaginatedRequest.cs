using Model.Request.Generic;

namespace Model.Request
{
    public class UserPaginatedRequest : GenericSearch
    {
        public UserPaginatedFilters Filters { get; set; }
    }

    public class UserPaginatedFilters : GenericPaginatedFilter
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
