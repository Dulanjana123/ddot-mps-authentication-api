using DDOT.MPS.Auth.Api.Managers;
using Microsoft.AspNetCore.Mvc;
using Model.Dtos;
using Model.Request.Generic;
using Model.Response;

namespace DDOT.MPS.Auth.Api.Controllers
{
    [ApiController]

    [Route("api/v1/RolePermissions")]
    public class PermissionsController : CoreController
    {
        private readonly IPermissionsManager _permissionsManager;

        public PermissionsController(IPermissionsManager permissionsManager)
        {
            _permissionsManager = permissionsManager;
        }

        // user groups
        [HttpGet("user-group/simple")]
        public async Task<IActionResult> GetAllUserGroups()
        {
            BaseResponse<List<UserGroupDto>> response = await _permissionsManager.GetAllUserGroups();
            return Ok(response);
        }

        [HttpPost("module-permissions")]
        public async Task<IActionResult> CreateModuleInterfacePermission([FromBody] ModuleInterfacePermissionDto permission)
        {
            if (permission == null)
            {
                return BadRequest("INVALID_MODULE_DATA");
            }
            BaseResponse<ModuleInterfacePermissionResponseDto> response = await _permissionsManager.CreateModuleInterfacePermission(permission);
            return Ok(response);
        }

        [HttpGet("role/next-code")]
        public async Task<IActionResult> GetNextRoleCode()
        {
            BaseResponse<string> response = await _permissionsManager.GetNextRoleCode();
            return Ok(response);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetModulesWithPermissionsAndInterfaces()
        {
            BaseResponse<List<ModuleUIDto>> response = await _permissionsManager.GetModulesWithPermissionsAndInterfaces();
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleDetailsWithPermissionsDto roleCreationDto)
        {
            BaseResponse<RoleResponseDto> response = await _permissionsManager.CreateRoleWithPermissions(roleCreationDto);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleWithPermissions(int id)
        {
            if (id <= 0)
            {
                return BadRequest("INVALID_ROLE_ID");
            }
            BaseResponse<RoleDto> response = await _permissionsManager.GetRoleWithPermissions(id);
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoleWithPermissions(int id, [FromBody] RoleUpdateDto roleUpdateDto)
        {
            if (id <= 0)
            {
                return BadRequest("INVALID_ROLE_ID");
            }
            if (roleUpdateDto == null)
            {
                return BadRequest("ROLE_DATA_REQUIRED");
            }

            BaseResponse<RoleDto> response = await _permissionsManager.UpdateRoleWithPermissions(id, roleUpdateDto);
            return Ok(response);
        }

        [HttpPost("role/paginated")]
        public async Task<IActionResult> GetAllRoleDetails([FromBody] RolePaginatedRequest request)
        {
            BaseResponse<Result<BaseRoleDto>> response = await _permissionsManager.GetAllRoleDetails(request);
            return Ok(response);
        }
    }
}
