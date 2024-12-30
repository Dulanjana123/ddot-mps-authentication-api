using AutoMapper;
using DataAccess.Contexts;
using DataAccess.Entities;
using Model.Dtos;

namespace DataAccess.Repositories
{
    public class MpsLoginHistoryRepository : IMpsLoginHistoryRepository
    { 
        private readonly MpsDbContext _dbContext;
        private readonly IMapper _mapper;

        public MpsLoginHistoryRepository(MpsDbContext mpsDbContext, IMapper mapper)
        {
            _dbContext = mpsDbContext ?? throw new ArgumentNullException(nameof(mpsDbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<FullLoginHistoryDto> CreateLoginHistory(FullLoginHistoryDto logHistory)
        {
            LoginHistory convertToDBObject = _mapper.Map<LoginHistory>(logHistory);
            await _dbContext.LoginHistories.AddAsync(convertToDBObject);
            await _dbContext.SaveChangesAsync();
            return logHistory;
        }
    }
}
