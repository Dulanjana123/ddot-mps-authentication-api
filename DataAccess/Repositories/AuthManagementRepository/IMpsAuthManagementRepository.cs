using DataAccess.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Model.Request.Generic;

namespace DataAccess.Repositories
{
    public interface IMpsAuthManagementRepository
    {        
        Task<IDbContextTransaction> CreateTransaction();

        Task<List<UserRole>> GetUserRolesWithPermissionsByUserId(int userId);

        Task<List<UserGroup>> GetAllActiveUserGroups();

        Task<Module> GetModuleById(int moduleId);

        Task<Interface> GetInterfaceById(int interfaceId);

        Task<MpsPermission> GetPermissionById(int permissionId);

        Task<bool> ModuleInterfacePermissionExists(int moduleId, int? interfaceId, int permissionId);

        Task<ModuleInterfacePermission> CreateModuleInterfacePermissionAsync(ModuleInterfacePermission permission);

        Task<List<Module>> GetAllActiveModules();

        Task<List<MpsPermission>> GetPermissionsByModuleId(int moduleId);

        Task<List<Interface>> GetInterfacesByModuleId(int moduleId);

        Task<List<ModuleInterfacePermission>> GetModuleInterfacePermissions(int moduleId, int? interfaceId);

        Task<Role> CreateRole(Role role);

        Task<RoleModuleInterfacePermission> CreateRoleModuleInterfacePermission(RoleModuleInterfacePermission permission);

        Task<bool> RoleExists(string code);

        Task<ModuleInterfacePermission> GetModuleInterfacePermissionById(int permissionId);

        Task<ModuleInterfacePermission> GetModuleInterfacePermissionByPermissionId(int permissionId);

        Task<ModuleInterfacePermission> GetModuleInterfacePermissionByInterfaceIdAndPermissionCode(int moduleId, int interfaceId, string permissionCode);

        Task<Role> GetRoleById(int roleId);

        Task<List<RoleModuleInterfacePermission>> GetRoleModuleInterfacePermissions(int roleId);

        Task<string> GetLatestRoleCode();

        Task UpdateRole(Role role);

        Task DeleteRoleModuleInterfacePermission(RoleModuleInterfacePermission permission);

        Task<List<UserRole>> GetUserRolesByUserId(int userId);

        Task<List<UserRole>> GetUserRolesAndPermissionsByUserId(int userId);
        IQueryable<Role> GetAllRoles(RolePaginatedRequest request);
        long GetRolesRowCount(RolePaginatedRequest request);
    }
}
