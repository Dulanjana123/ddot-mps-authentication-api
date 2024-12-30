using DDOT.MPS.Auth.Api.Managers;
using Microsoft.AspNetCore.Mvc;
using Model.Dtos;
using Model.Response;

namespace DDOT.MPS.Auth.Api.Controllers
{
    [ApiController]
    public class AuthController : CoreController
    {
        private readonly IAuthManager _authManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _environmentName;

        public AuthController(IAuthManager authManager, ILogger<AuthController> logger, IConfiguration configuration, string environmentName) 
        { 
            _authManager = authManager;
            _logger = logger;
            _configuration = configuration;
            _environmentName = environmentName;
        }

        [HttpGet("check-connection-string")]
        public async Task<IActionResult> CheckConnectionString()
        {
            _logger.LogInformation("DDOT.MPS.Auth.Api.Controllers.AuthController.CheckConnectionString | Request in progress.");
            string? connectionString = _configuration.GetConnectionString("DefaultConnection");
            _logger.LogInformation("DDOT.MPS.Auth.Api.Controllers.AuthController.CheckConnectionString | Environment: {environmentName} | Connection string: {connectionString}", _environmentName, connectionString);
            return Ok($"{_environmentName} : {connectionString}");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            BaseResponse<B2CTokenResponse> loginResponse = await _authManager.Login(userLoginDto);
            return Ok(loginResponse);
        }

        [HttpPost("login-v2")]
        public async Task<IActionResult> LoginV2([FromBody] UserLoginDto userLoginDto)
        {
            BaseResponse<OtpGenerateResponseDto> loginResponse = await _authManager.LoginV2(userLoginDto);
            return Ok(loginResponse);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            BaseResponse<ResetPasswordResponseDto> createdUser = await _authManager.ResetPassword(resetPasswordDto);
            return Ok(createdUser);
        }

        [HttpGet("user-check")]
        public async Task<IActionResult> UserCheck([FromQuery] string email)
        {
            _logger.LogInformation("DDOT.MPS.Auth.Api.Controllers.AuthController.UserCheck | Request in progress. Email: {email}", email);
            BaseResponse<UserResponseDto> createdUser = await _authManager.UserCheck(email);
            return Ok(createdUser);
        }

        [HttpGet("user-reset-check")]
        public async Task<IActionResult> UserInitialResetCheck([FromQuery] string emailToken)
        {
            BaseResponse<string> createdUser = await _authManager.UserIsInitialPasswordResetCheck(emailToken);
            return Ok(createdUser);
        }

        [HttpGet("generate-otp")]
        public async Task<IActionResult> GenerateOTP([FromQuery] string email)
        {
            BaseResponse<OtpGenerateResponseDto> createdUser = await _authManager.GenerateOTP(email);
            return Ok(createdUser);
        }
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromBody] OtpGenerateDto request)
        {
            BaseResponse<OtpVerifyResponseDto> createdUser = await _authManager.VerifyOtp(request.Email, request.Otp);
            return Ok(createdUser);
        }

        [HttpGet("get-reset-password-token")]
        public async Task<IActionResult> GetResetPasswordToken([FromQuery] string email)
        {
            BaseResponse<UserTokenResponse> token = await _authManager.GenerateResetPasswordToken(email);
            return Ok(token);
        }

        [HttpGet("get-access-token")]
        public async Task<IActionResult> GetAccessToken([FromQuery] string email)
        {
            BaseResponse<UserRolesWithPermissionsAccessToken> token = await _authManager.GenerateAccessToken(email);
            return Ok(token);
        }
        [HttpGet("get-user-roles-permissions")]
        public async Task<IActionResult> GetAccessToken([FromQuery] int userId)
        {
            var  userRolesPermissions = await _authManager.GetRolesAndPermissionsByUserId(userId);
            return Ok(userRolesPermissions);
        }
    }
}
