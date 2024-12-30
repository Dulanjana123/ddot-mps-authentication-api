using DataAccess.Entities;
using Model.Dtos;
using Model.Response;

namespace DDOT.MPS.Auth.Api.Managers
{
    public interface ILoginHistoryManager
    {
        Task<BaseResponse<FullLoginHistoryDto>> CreateLoginHistory(LoginHistoryBaseDto loginHistory, LoginHistoryHeaderDto headers);
    }
}
