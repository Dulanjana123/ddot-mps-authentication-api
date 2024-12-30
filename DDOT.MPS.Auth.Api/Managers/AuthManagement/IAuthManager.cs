using Model.Dtos;
using Model.Response;

namespace DDOT.MPS.Auth.Api.Managers
{
    public interface IAuthManager
    {
        Task<BaseResponse<ResetPasswordResponseDto>> ResetPassword(ResetPasswordDto resetPasswordDto);
        Task<BaseResponse<B2CTokenResponse>> Login(UserLoginDto userLoginDto);
        Task<BaseResponse<OtpGenerateResponseDto>> LoginV2(UserLoginDto userLoginDto);
        Task<BaseResponse<UserResponseDto>> UserCheck(string email);
        Task<BaseResponse<string>> UserIsInitialPasswordResetCheck(string emailToken);
        Task<BaseResponse<OtpGenerateResponseDto>> GenerateOTP(string email);
        Task<BaseResponse<OtpVerifyResponseDto>> VerifyOtp(string email, int otp);
        Task<BaseResponse<UserTokenResponse>> GenerateResetPasswordToken(string email);
        Task<BaseResponse<UserRolesWithPermissionsAccessToken>> GenerateAccessToken (string email);
        Task<BaseResponse<List<UserRoleWithPermissionsDto>>> GetRolesAndPermissionsByUserId(int userId);
    }
}
