using Core.Enums;
using DDOT.MPS.Auth.Api.Managers;
using Microsoft.AspNetCore.Mvc;
using Model.Dtos;
using Model.Request;
using Model.Response;

namespace DDOT.MPS.Auth.Api.Controllers
{
    [ApiController]
    public class UserController : CoreController
    {
        private readonly IUserManager _userManager;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserManager userManager, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
        {
            _logger.LogInformation("DDOT.MPS.Auth.Api.Controllers.UserController.CreateUser | Request in progress. Requested user object: {userDto}", userDto);
            return Ok(await _userManager.CreateUser(userDto));
        }

        [HttpPost("validate-otp")]
        public async Task<IActionResult> CreateUser(ValidateOtpDto validateOtpDto)
        {
            BaseResponse<UserResponseDto> response = await _userManager.ValidateOTP(validateOtpDto.UserId, validateOtpDto.Otp);
            return Ok(response);
        }

        [HttpPost("paginated")]
        public async Task<IActionResult> GetPaginatedList([FromBody] UserPaginatedRequest request)
        {
            BaseResponse<Result<UserDetailsDto>> updatedUser = await _userManager.GetPaginatedList(request);
            return Ok(updatedUser);
        }

        [HttpGet("get-user-types-and-agencies")]
        public async Task<IActionResult> GetUserTypesAndAgencies()
        {
            BaseResponse<UserTypesAndAgenciesDto> response = await _userManager.GetUserTypesAndAgencies();
            return Ok(response);
        }
    }
}
