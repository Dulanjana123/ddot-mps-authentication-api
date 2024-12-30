using DataAccess.Entities;

namespace DataAccess.Repositories
{
    public interface IMpsAgencyCategoryRepository
    {
        Task<AgencyCategory?> GetAgencyCategoryByAgencyCategoryCode(string agencyCategoryCode);        
    }
}
