using DDOT.MPS.Auth.Api.Managers;
using Microsoft.AspNetCore.Mvc;
using Model.Dtos;
using Model.Request;
using Model.Response;

namespace DDOT.MPS.Auth.Api.Controllers
{
    [ApiController]
    public class AgencyController : CoreController
    {
        private readonly IAgencyManager _agencyManager;

        public AgencyController(IAgencyManager agencyManager)
        {
            _agencyManager = agencyManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAgency([FromBody] AgencyDto agencyDto)
        {
            BaseResponse<AgencyResponseDto> createdAgency = await _agencyManager.CreateAgency(agencyDto);
            return Ok(createdAgency);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAgencyById(int id)
        {
            BaseResponse<AgencyResponseDto> response = await _agencyManager.GetAgencyById(id);
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAgency(int id, [FromBody] AgencyDto agencyDto)
        {
            BaseResponse<AgencyResponseDto> updatedAgency = await _agencyManager.UpdateAgency(id, agencyDto);
            return Ok(updatedAgency);
        }

        [HttpPost("paginated")]
        public async Task<IActionResult> GetPaginatedList([FromBody] AgencyPaginatedRequest request)
        {
            BaseResponse<Result<AgencyResponseDto>> updatedAgency = await _agencyManager.GetPaginatedList(request);
            return Ok(updatedAgency);
        }
    }
}
