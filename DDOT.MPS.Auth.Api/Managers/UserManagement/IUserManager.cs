using Model.Dtos;
using Model.Request;
using Model.Response;

namespace DDOT.MPS.Auth.Api.Managers
{
    public interface IUserManager
    {
        Task<BaseResponse<UserResponseDto>> CreateUser(UserDto user);
        Task<BaseResponse<UserResponseDto>> CreateAdmin(UserDto user);
        Task<BaseResponse<UserResponseDto>> ValidateOTP(int userId, int otp);
        Task<BaseResponse<Result<UserDetailsDto>>> GetPaginatedList(UserPaginatedRequest request);
        Task<BaseResponse<UserTypesAndAgenciesDto>> GetUserTypesAndAgencies();
    }
}
