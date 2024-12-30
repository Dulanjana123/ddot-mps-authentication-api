using AutoMapper;
using DataAccess.Contexts;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class MpsAgencyCategoryRepository : IMpsAgencyCategoryRepository
    {
        private readonly MpsDbContext _dbContext;
        private readonly IMapper _mapper;

        public MpsAgencyCategoryRepository(MpsDbContext mpsDbContext, IMapper mapper)
        {
            _dbContext = mpsDbContext ?? throw new ArgumentNullException(nameof(mpsDbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<AgencyCategory?> GetAgencyCategoryByAgencyCategoryCode(string agencyCategoryCode)
        {
            return await _dbContext.AgencyCategories
                .Where(t => t.AgencyCategoryCode == agencyCategoryCode)
                .FirstOrDefaultAsync();
        }
    }
}
