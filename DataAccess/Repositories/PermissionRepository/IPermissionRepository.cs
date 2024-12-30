using Model.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public interface IPermissionRepository
    {
        Task<List<ModuleUIDto>> GetModulesWithInterfacesAndPermissionsAsync();
        Task<bool> ModuleInterfacePermissionExists(int moduleInterfacePermissionId);
        Task AddRoleModuleInterfacePermission(List<RoleModuleInterfacePermissionDto> roleModuleInterfacePermissionList);

    }
}
