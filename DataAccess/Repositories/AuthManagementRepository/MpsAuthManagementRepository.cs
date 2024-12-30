using AutoMapper;
using DataAccess.Contexts;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Model.Request.Generic;

namespace DataAccess.Repositories
{
    public class MpsAuthManagementRepository : IMpsAuthManagementRepository
    {        
        private readonly MpsDbContext _dbContext;
        private readonly IMapper _mapper;

        public MpsAuthManagementRepository(MpsDbContext mpsDbContext, IMapper mapper)
        {
            _dbContext = mpsDbContext ?? throw new ArgumentNullException(nameof(mpsDbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // transaction
        public async Task<IDbContextTransaction> CreateTransaction()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }

        // user groups
        public async Task<List<UserGroup>> GetAllActiveUserGroups()
        {
            return await _dbContext.UserGroups
                .Where(p => p.IsActive == true)
                .ToListAsync();
        }

        public async Task<List<UserRole>> GetUserRolesWithPermissionsByUserId(int userId)
        {
            return await _dbContext.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.IsActive == true)
                .ToListAsync();
        }

        public async Task<Module> GetModuleById(int moduleId)
        {
            return await _dbContext.Modules.FindAsync(moduleId);
        }

        public async Task<Interface> GetInterfaceById(int interfaceId)
        {
            return await _dbContext.Interfaces.FindAsync(interfaceId);
        }

        public async Task<MpsPermission> GetPermissionById(int permissionId)
        {
            return await _dbContext.Permissions.FindAsync(permissionId);
        }

        public async Task<bool> ModuleInterfacePermissionExists(int moduleId, int? interfaceId, int permissionId)
        {
            return await _dbContext.ModuleInterfacePermissions
                .AnyAsync(mip => mip.ModuleId == moduleId
                    && mip.InterfaceId == interfaceId
                    && mip.PermissionId == permissionId);
        }
        public async Task<ModuleInterfacePermission> CreateModuleInterfacePermissionAsync(ModuleInterfacePermission permission)
        {
            await _dbContext.ModuleInterfacePermissions.AddAsync(permission);
            await _dbContext.SaveChangesAsync();

            return permission;
        }

        public async Task<List<Module>> GetAllActiveModules()
        {
            return await _dbContext.Modules.Include(i=>i.ModuleInterfacePermissions).Where(m => m.IsActive == true).ToListAsync();
        }

        public async Task<List<MpsPermission>> GetPermissionsByModuleId(int moduleId)
        {
            return await _dbContext.Permissions
                .Where(p => p.ModuleInterfacePermissions.Any(mip => mip.ModuleId == moduleId))
                .ToListAsync();
        }
       
        public async Task<List<Interface>> GetInterfacesByModuleId(int moduleId)
        {
            return await _dbContext.ModuleInterfacePermissions
                .Where(mip => mip.ModuleId == moduleId && mip.InterfaceId.HasValue)
                .Select(mip => mip.Interface)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<ModuleInterfacePermission>> GetModuleInterfacePermissions(int moduleId, int? interfaceId)
        {
            return await _dbContext.ModuleInterfacePermissions
                .Where(mip => mip.ModuleId == moduleId && mip.InterfaceId == interfaceId)
                .Include(mip => mip.Permission)
                .ToListAsync();
        }

        public async Task<Role> CreateRole(Role role)
        {
            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync();
            return role;
        }

        public async Task<RoleModuleInterfacePermission> CreateRoleModuleInterfacePermission(RoleModuleInterfacePermission permission)
        {
            _dbContext.RoleModuleInterfacePermissions.Add(permission);
            await _dbContext.SaveChangesAsync();
            return permission;
        }

        public async Task<bool> RoleExists(string code)
        {
            return await _dbContext.Roles.AnyAsync(r => r.Code == code );
        }

        public async Task<ModuleInterfacePermission> GetModuleInterfacePermissionById(int permissionId)
        {
            return await _dbContext.ModuleInterfacePermissions.FindAsync(permissionId);
        }

        public async Task<ModuleInterfacePermission> GetModuleInterfacePermissionByPermissionId(int permissionId)
        {
            return await _dbContext.ModuleInterfacePermissions
                .FirstOrDefaultAsync(mip => mip.PermissionId == permissionId);
        }

        public async Task<ModuleInterfacePermission> GetModuleInterfacePermissionByInterfaceIdAndPermissionCode(int moduleId, int interfaceId, string permissionCode)
        {
            return await _dbContext.ModuleInterfacePermissions
                .Include(mip => mip.Permission)
                .FirstOrDefaultAsync(mip => mip.ModuleId == moduleId && mip.InterfaceId == interfaceId && mip.Permission.Code == permissionCode);
        }

        public async Task<Role> GetRoleById(int roleId)
        {
            return await _dbContext.Roles
                .Include(r => r.RoleModuleInterfacePermissions)
                    .ThenInclude(rmip => rmip.ModuleInterfacePermission)
                        .ThenInclude(mip => mip.Permission)
                .Include(r => r.RoleModuleInterfacePermissions)
                    .ThenInclude(rmip => rmip.ModuleInterfacePermission)
                        .ThenInclude(mip => mip.Interface)
                .Include(r => r.UserGroup)
                .FirstOrDefaultAsync(r => r.RoleId == roleId);
        }

        public async Task<List<RoleModuleInterfacePermission>> GetRoleModuleInterfacePermissions(int roleId)
        {
            return await _dbContext.RoleModuleInterfacePermissions
                .Include(rmip => rmip.ModuleInterfacePermission)
                    .ThenInclude(mip => mip.Module)
                .Include(rmip => rmip.ModuleInterfacePermission)
                    .ThenInclude(mip => mip.Permission)
                .Include(rmip => rmip.ModuleInterfacePermission)
                    .ThenInclude(mip => mip.Interface)
                .Where(rmip => rmip.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<string> GetLatestRoleCode()
        {
            int latestRoleCode = await _dbContext.Roles
                .OrderByDescending(r => r.RoleId)
                .Select(r => r.RoleId)
                .FirstOrDefaultAsync();

            return latestRoleCode.ToString();
        }

        public async Task UpdateRole(Role role)
        {
            _dbContext.Roles.Update(role);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteRoleModuleInterfacePermission(RoleModuleInterfacePermission permission)
        {
            _dbContext.RoleModuleInterfacePermissions.Remove(permission);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<UserRole>> GetUserRolesByUserId(int userId)
        {
            return await _dbContext.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.IsActive == true)
                .ToListAsync();
        }
        public async Task<List<UserRole>> GetUserRolesAndPermissionsByUserId(int userId)
        {
            return await _dbContext.UserRoles
                .Include(ur => ur.Role)
                .ThenInclude(ur => ur.RoleModuleInterfacePermissions)
                .ThenInclude(p=>p.ModuleInterfacePermission)
                .ThenInclude(p=>p.Permission)
                .Where(ur => ur.UserId == userId && ur.IsActive == true)
                .ToListAsync();
        }

        public IQueryable<Role> GetAllRoles(RolePaginatedRequest request)
        {
            string? userRoleId = request.Filters.UserRoleId;
            string? userRoleDescription = request.Filters.UserRoleDescription;

            IQueryable<Role> roles = _dbContext.Roles
                .Where(r => (string.IsNullOrEmpty(userRoleId) || r.Code.Trim().ToLower().Contains(userRoleId)) && (string.IsNullOrEmpty(userRoleDescription) || r.Description.Trim().ToLower().Contains(userRoleDescription)))
                .OrderByDescending(r => r.ModifiedDate.HasValue ? r.ModifiedDate : r.CreatedDate)
                .Skip(request.PagingAndSortingInfo.Paging.PageSize * (request.PagingAndSortingInfo.Paging.PageNo - 1))
                .Take(request.PagingAndSortingInfo.Paging.PageSize);

            return roles;
        }

        public long GetRolesRowCount(RolePaginatedRequest request)
        {
            string? userRoleId = request.Filters.UserRoleId;
            string? userRoleDescription = request.Filters.UserRoleDescription;

            long rolesCount = _dbContext.Roles
                .Where(r => (string.IsNullOrEmpty(userRoleId) || r.Code.Trim().ToLower().Contains(userRoleId)) && (string.IsNullOrEmpty(userRoleDescription) || r.Description.Trim().ToLower().Contains(userRoleDescription)))
                .LongCount();

            return rolesCount;
        }
    }
}
