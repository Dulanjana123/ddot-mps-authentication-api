using Model.Dtos;
using Model.Request.Generic;
using Model.Response;

namespace DDOT.MPS.Auth.Api.Managers
{
    public interface IPermissionsManager
    {
        Task<BaseResponse<List<UserGroupDto>>> GetAllUserGroups();

        Task<BaseResponse<ModuleInterfacePermissionResponseDto>> CreateModuleInterfacePermission(ModuleInterfacePermissionDto permission);

        Task<BaseResponse<List<ModuleUIDto>>> GetModulesWithPermissionsAndInterfaces();

        Task<BaseResponse<RoleResponseDto>> CreateRoleWithPermissions(RoleDetailsWithPermissionsDto rolePermissionCreationDto);

        Task<BaseResponse<RoleDto>> GetRoleWithPermissions(int roleId);

        Task<BaseResponse<string>> GetNextRoleCode();

        Task<BaseResponse<RoleDto>> UpdateRoleWithPermissions(int roleId, RoleUpdateDto roleUpdateDto);
        Task<BaseResponse<Result<BaseRoleDto>>> GetAllRoleDetails(RolePaginatedRequest request);
    }
}
