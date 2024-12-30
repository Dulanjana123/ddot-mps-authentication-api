using Model.Dtos;

namespace DataAccess.Repositories
{
    public interface IMpsLoginHistoryRepository
    {        
        Task<FullLoginHistoryDto> CreateLoginHistory(FullLoginHistoryDto logHistory);        
    }
}
