using AutoMapper;
using DataAccess.Contexts;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Model.Dtos;
using Model.Request;
using Model.Response;

namespace DataAccess.Repositories
{
    public class MpsAgencyRepository : IMpsAgencyRepository
    {
        private readonly MpsDbContext _dbContext;
        private readonly IMapper _mapper;

        public MpsAgencyRepository(MpsDbContext mpsDbContext, IMapper mapper)
        {
            _dbContext = mpsDbContext ?? throw new ArgumentNullException(nameof(mpsDbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<AgencyDto> CreateAgency(Agency agency)
        {
            _dbContext.Add(agency);
            await _dbContext.SaveChangesAsync();
            AgencyDto newAgency = _mapper.Map<AgencyDto>(agency);
            return newAgency;
        }

        public async Task<Agency?> GetAgencyById(int id)
        {
            return await _dbContext.Agencies.Where(a => a.AgencyId == id).FirstOrDefaultAsync();
        }

        public async Task<Agency?> GetAgencyByAgencyCode(string agencyCode)
        {
            return await _dbContext.Agencies.Where(a => a.AgencyCode == agencyCode).FirstOrDefaultAsync();
        }

        public async Task<AgencyDto> UpdateAgency(Agency agency)
        {
            _dbContext.Update(agency);
            await _dbContext.SaveChangesAsync();
            AgencyDto newAgency = _mapper.Map<AgencyDto>(agency);
            return newAgency;
        }

        public IQueryable<Agency> GetAll(AgencyPaginatedRequest request)
        {
            IQueryable<Agency> agencies = (
                from agency in _dbContext.Agencies

                where
                agency.AgencyCode.Contains(request.Filters.AgencyCode.Trim())
                && agency.AgencyName.Contains(request.Filters.AgencyName.Trim())

                orderby agency.ModifiedDate.HasValue ? agency.ModifiedDate : agency.CreatedDate descending

                select agency
            )
            .Skip((request.PagingAndSortingInfo.Paging.PageNo - 1) * request.PagingAndSortingInfo.Paging.PageSize)
            .Take(request.PagingAndSortingInfo.Paging.PageSize)
            .AsQueryable();

            return agencies;
        }

        public long GetRowCount(AgencyPaginatedRequest request)
        {
            return _dbContext.Agencies.Where(x =>
                x.AgencyCode.Contains(request.Filters.AgencyCode.Trim())
                && x.AgencyName.Contains(request.Filters.AgencyName.Trim())
            ).Count();
        }

        public UserTypesAndAgenciesDto GetUserTypesAndAgencies()
        {
            List<Agency> agency = _dbContext.Agencies.ToList();
            List<UserType> userType = _dbContext.UserTypes.ToList();

            UserTypesAndAgenciesDto userTypesAndAgencies = new UserTypesAndAgenciesDto
            {
                Agencies = _mapper.Map<List<AgencyComboDto>>(agency),
                UserTypes = _mapper.Map<List<UserTypeComboDto>>(userType)
            };

            return userTypesAndAgencies;
        }
    }
}
