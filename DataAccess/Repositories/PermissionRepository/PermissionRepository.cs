using AutoMapper;
using DataAccess.Contexts;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Model.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly MpsDbContext _dbContext;
        private readonly IMapper _mapper;

        public PermissionRepository(MpsDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public async Task<List<ModuleUIDto>> GetModulesWithInterfacesAndPermissionsAsync()
        {
            List<Module> moduleData = await _dbContext.Modules
                .Include(m => m.ModuleInterfacePermissions)
                    .ThenInclude(mip => mip.Interface)
                .Include(m => m.ModuleInterfacePermissions)
                    .ThenInclude(mip => mip.Permission)
                .Where(m => m.IsActive)
                .ToListAsync();

            return moduleData
                .GroupBy(m => new
                {
                    m.ModuleId,
                    m.Code,
                    m.Name,
                    m.Description,
                    m.SortId,
                    m.IsActive
                })
                .OrderBy(moduleGroup => moduleGroup.Key.SortId)
                .Select(moduleGroup => new ModuleUIDto
                {
                    ModuleId = moduleGroup.Key.ModuleId,
                    Code = moduleGroup.Key.Code,
                    Name = moduleGroup.Key.Name,
                    Description = moduleGroup.Key.Description,
                    SortId = moduleGroup.Key.SortId,
                    IsActive = moduleGroup.Key.IsActive,
                    Interfaces = moduleGroup
                        .SelectMany(m => m.ModuleInterfacePermissions)
                        .GroupBy(mip => new
                        {
                            mip.Interface.InterfaceId,
                            mip.Interface.Code,
                            mip.Interface.Name,
                            mip.Interface.SortId
                        })
                        .OrderBy(interfaceGroup => interfaceGroup.Key.SortId)
                        .Select(interfaceGroup => new InterfaceUIDto
                        {
                            InterfaceId = interfaceGroup.Key.InterfaceId,
                            Code = interfaceGroup.Key.Code,
                            Name = interfaceGroup.Key.Name,
                            SortId = interfaceGroup.Key.SortId,
                            Permissions = interfaceGroup
                                .Select(p => new PermissionUIDto
                                {
                                    PermissionId = p.Permission.PermissionId,
                                    Code = p.Permission.Code,
                                    Name = p.Permission.Name,
                                    Description = p.Permission.Description,
                                    SortId = p.Permission.SortId,
                                    Checked = p.IsEnabled ?? false,
                                    IsActive = p.Permission.IsActive,
                                    IsCrud = p.Permission.IsCrud,
                                    ModuleInterfacePermissionId = p.ModuleInterfacePermissionId
                                })
                                .OrderBy(p => p.SortId)
                                .ToList()
                        }).ToList()
                }).ToList();

        }
        public async Task<bool> ModuleInterfacePermissionExists(int moduleInterfacePermissionId)
        {
            return await _dbContext.ModuleInterfacePermissions.AnyAsync(r => r.ModuleInterfacePermissionId == moduleInterfacePermissionId && r.IsActive);
        }
        public async Task AddRoleModuleInterfacePermission(List<RoleModuleInterfacePermissionDto> roleModuleInterfacePermissionList)
        {
            List<RoleModuleInterfacePermission> roleModuleInterfacePermissions =
                        _mapper.Map<List<RoleModuleInterfacePermission>>(roleModuleInterfacePermissionList);

            await _dbContext.RoleModuleInterfacePermissions.AddRangeAsync(roleModuleInterfacePermissions);
            await _dbContext.SaveChangesAsync();
        }
    }
}
