using DataAccess.Entities;
using Model.Dtos;
using Model.Request;
using Model.Response;

namespace DataAccess.Repositories
{
    public interface IMpsAgencyRepository
    {
        Task<AgencyDto> CreateAgency(Agency agency);

        Task<AgencyDto> UpdateAgency(Agency agency);

        Task<Agency> GetAgencyById(int id);

        Task<Agency> GetAgencyByAgencyCode(string agencyCode);

        IQueryable<Agency> GetAll(AgencyPaginatedRequest request);

        long GetRowCount(AgencyPaginatedRequest request);
        UserTypesAndAgenciesDto GetUserTypesAndAgencies();
    }
}
