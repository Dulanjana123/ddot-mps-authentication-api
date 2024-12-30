using DDOT.MPS.Auth.Api.Managers;
using Microsoft.AspNetCore.Mvc;
using Model.Dtos;
using Model.Response;

namespace DDOT.MPS.Auth.Api.Controllers
{
    [ApiController]
    public class LoginHistoryController : CoreController
    {
        private readonly ILoginHistoryManager _loginHistoryManager;
        public LoginHistoryController(ILoginHistoryManager loginHistoryManager)
        {
            _loginHistoryManager = loginHistoryManager;
        }

        [HttpPost("create-login-history")]
        public async Task<IActionResult> CreateLoginHistory([FromBody] LoginHistoryBaseDto loginHistory)
        {
            var loginHistoryHeaders = new LoginHistoryHeaderDto
            {
                IpAddress = Request.Headers["ipaddress"].FirstOrDefault(),
                Agent = Request.Headers["agent"].FirstOrDefault(),
            };
            BaseResponse<FullLoginHistoryDto> createdLoginHistory = await _loginHistoryManager.CreateLoginHistory(loginHistory, loginHistoryHeaders);
            return Ok(createdLoginHistory);
        }
    }
}
