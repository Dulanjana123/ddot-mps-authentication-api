using Model.Dtos;
using Model.Request;
using Model.Response;

namespace DDOT.MPS.Auth.Api.Managers
{
    public interface IAgencyManager
    {
        Task<BaseResponse<AgencyResponseDto>> CreateAgency(AgencyDto agency);

        Task<BaseResponse<AgencyResponseDto>> GetAgencyById(int id);

        Task<BaseResponse<AgencyResponseDto>> UpdateAgency(int id, AgencyDto agency);

        Task<BaseResponse<Result<AgencyResponseDto>>> GetPaginatedList(AgencyPaginatedRequest request);
    }
}
