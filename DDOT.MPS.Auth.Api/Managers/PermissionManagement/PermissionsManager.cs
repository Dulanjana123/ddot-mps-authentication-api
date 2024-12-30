using AutoMapper;
using Core.CoreSettings;
using Core.Utilities;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Model.Dtos;
using Model.Request.Generic;
using Model.Response;
using System.Collections.Generic;
using System.Data;

namespace DDOT.MPS.Auth.Api.Managers

{
    public class PermissionsManager : IPermissionsManager
    {
        private readonly IMapper _mapper;
        private readonly IMpsAuthManagementRepository _authManagementRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IMpsUserRepository _userRepository;
        private readonly GlobalAppSettings _globalAppSettings;
        private readonly IAppUtils _appUtils;

        public PermissionsManager(IMpsAuthManagementRepository authManagementRepository, IMpsUserRepository userRepository, IMapper mapper, IOptions<GlobalAppSettings> globalAppSettings, IAppUtils appUtils, IPermissionRepository permissionRepository)
        {
            _authManagementRepository = authManagementRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _globalAppSettings = globalAppSettings.Value;
            _appUtils = appUtils;
            _permissionRepository = permissionRepository;
        }

        // user groups
        public async Task<BaseResponse<List<UserGroupDto>>> GetAllUserGroups()
        {
            List<UserGroup> activeUserGroups = await _authManagementRepository.GetAllActiveUserGroups();

            if (activeUserGroups == null || !activeUserGroups.Any())
            {
                return new BaseResponse<List<UserGroupDto>>
                {
                    Success = false,
                    Data = null,
                    Message = "NO_USER_GROUPS_FOUND"
                };
            }

            List<UserGroupDto> userGroupDtos = activeUserGroups.Select(ug => new UserGroupDto
            {
                UserGroupId = ug.UserGroupId,
                Code = ug.Code,
                Description = ug.Description,
                Name = ug.Name,
                SortId = ug.SortId
            }).ToList();

            return new BaseResponse<List<UserGroupDto>>
            {
                Success = true,
                Data = userGroupDtos,
                Message = "USER_GROUPS_RETRIEVED_SUCCESSFULLY"
            };
        }

        // create module interface and permissions
        public async Task<BaseResponse<ModuleInterfacePermissionResponseDto>> CreateModuleInterfacePermission(ModuleInterfacePermissionDto permission)
        {
            try
            {
                Module module = await _authManagementRepository.GetModuleById(permission.ModuleId);
                if (module == null)
                {
                    return new BaseResponse<ModuleInterfacePermissionResponseDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "INVALID_MODULE_ID"
                    };
                }

                Interface? iface = null;
                if (permission.InterfaceId.HasValue)
                {
                    iface = await _authManagementRepository.GetInterfaceById(permission.InterfaceId.Value);
                    if (iface == null)
                    {
                        return new BaseResponse<ModuleInterfacePermissionResponseDto>
                        {
                            Success = false,
                            Data = null,
                            Message = "INVALID_INTERFACE_ID"
                        };
                    }
                }

                MpsPermission permissionEntity = await _authManagementRepository.GetPermissionById(permission.PermissionId);
                if (permissionEntity == null)
                {
                    return new BaseResponse<ModuleInterfacePermissionResponseDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "INVALID_PERMISSION_ID"
                    };
                }

                bool permissionExists = await _authManagementRepository.ModuleInterfacePermissionExists(permission.ModuleId, permission.InterfaceId, permission.PermissionId);
                if (permissionExists)
                {
                    return new BaseResponse<ModuleInterfacePermissionResponseDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "MODULE_PERMISSION_ALREADY_EXISTS"
                    };
                }

                ModuleInterfacePermission mpsPermission = _mapper.Map<ModuleInterfacePermission>(permission);

                if (iface != null)
                {
                    mpsPermission.Code = $"{module.Code}_{iface.Code}_{permissionEntity.Code}";
                }
                else
                {
                    mpsPermission.Code = $"{module.Code}_{permissionEntity.Code}";
                }

                mpsPermission.CreatedBy = 189; // NEED TO BE REPLACED WITH AUTHORIZATION

                ModuleInterfacePermission createdPermission = await _authManagementRepository.CreateModuleInterfacePermissionAsync(mpsPermission);
                ModuleInterfacePermissionResponseDto createdPermissionResponse = _mapper.Map<ModuleInterfacePermissionResponseDto>(createdPermission);

                return new BaseResponse<ModuleInterfacePermissionResponseDto>
                {
                    Success = true,
                    Data = createdPermissionResponse,
                    Message = "MODULE_INTERFACE_PERMISSION_CREATED_SUCCESSFULLY"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ModuleInterfacePermissionResponseDto>
                {
                    Success = false,
                    Data = null,
                    Message = $"ERROR_CREATING_MODULE_INTERFACE_PERMISSION: {ex.Message}"
                };
            }
        }

        // get modules with interfaces and permissions
        public async Task<BaseResponse<List<ModuleUIDto>>> GetModulesWithPermissionsAndInterfaces()
        {
            List<ModuleUIDto> result = await _permissionRepository.GetModulesWithInterfacesAndPermissionsAsync();
            return new BaseResponse<List<ModuleUIDto>> { Success = true, Data = result, Message = "MODULE_INTERFACE_PERMISSIONS_RETRIVED" };
        }

        // get next role code
        public async Task<BaseResponse<string>> GetNextRoleCode()
        {
            try
            {
                string latestRoleCode = await _authManagementRepository.GetLatestRoleCode();

                if (string.IsNullOrEmpty(latestRoleCode))
                {
                    return new BaseResponse<string>
                    {
                        Success = true,
                        Data = "001",
                        Message = "NEXT_ROLE_CODE_GENERATED_SUCCESSFULLY"
                    };
                }

                if (int.TryParse(latestRoleCode, out int currentMaxCode))
                {
                    string nextRoleCode = (currentMaxCode + 1).ToString("D3");
                    return new BaseResponse<string>
                    {
                        Success = true,
                        Data = nextRoleCode,
                        Message = "NEXT_ROLE_CODE_GENERATED_SUCCESSFULLY"
                    };
                }

                return new BaseResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = "ERROR_PARSING_ROLE_CODE"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = "ERROR_GENERATING_NEXT_ROLE_CODE"
                };
            }
        }

        public async Task<BaseResponse<RoleResponseDto>> CreateRoleWithPermissions(RoleDetailsWithPermissionsDto rolePermissionCreationDto)
        {

            bool roleExists = await _authManagementRepository.RoleExists(rolePermissionCreationDto.Code);
            if (roleExists)
            {
                return new BaseResponse<RoleResponseDto>
                {
                    Success = false,
                    Data = null,
                    Message = "ROLE_ALREADY_EXISTS"
                };
            }

            Role role = _mapper.Map<Role>(rolePermissionCreationDto);
            role = await _authManagementRepository.CreateRole(role);

            if (rolePermissionCreationDto.Permissions != null)
            {
                List<RoleModuleInterfacePermissionDto> rolePermissions = new List<RoleModuleInterfacePermissionDto>();
                foreach (int moduleInterfacePermissionId in rolePermissionCreationDto.Permissions)
                {
                    if (await _permissionRepository.ModuleInterfacePermissionExists(moduleInterfacePermissionId))
                    {
                        rolePermissions.Add(new RoleModuleInterfacePermissionDto { ModuleInterfacePermissionId = moduleInterfacePermissionId, RoleId = role.RoleId });
                    }
                }

                await _permissionRepository.AddRoleModuleInterfacePermission(rolePermissions);
            }
            RoleResponseDto roleResponseDto = _mapper.Map<RoleResponseDto>(role);
            return new BaseResponse<RoleResponseDto>
            {
                Success = true,
                Data = roleResponseDto,
                Message = "ROLE_CREATED_SUCCESSFULLY"
            };
        }

        public async Task<BaseResponse<RoleDto>> GetRoleWithPermissions(int roleId)
        {
            try
            {
                Role role = await _authManagementRepository.GetRoleById(roleId);
                if (role == null)
                {
                    return new BaseResponse<RoleDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "ROLE_NOT_FOUND"
                    };
                }

                RoleDto roleDto = _mapper.Map<RoleDto>(role);

                List<RoleModuleInterfacePermission> roleModuleInterfacePermissions = await _authManagementRepository.GetRoleModuleInterfacePermissions(roleId);

                List<Module> allModules = await _authManagementRepository.GetAllActiveModules();

                foreach (Module module in allModules)
                {
                    ModuleDto moduleDto = new ModuleDto
                    {
                        ModuleId = module.ModuleId,
                        Code = module.Code,
                        Name = module.Name,
                        Description = module.Description,
                        SortId = module.SortId,
                        IsActive = module.IsActive
                    };

                    List<MpsPermission> permissions = await _authManagementRepository.GetPermissionsByModuleId(module.ModuleId);
                    List<MpsPermission> crudPermissions = permissions.Where(p => p.IsCrud == true).ToList();
                    List<MpsPermission> otherPermissions = permissions.Where(p => p.IsCrud == false).ToList();

                    moduleDto.OtherPermissions = otherPermissions.Select(p => new PermissionDto
                    {
                        PermissionId = p.PermissionId,
                        Name = p.Name,
                        Code = p.Code
                    }).ToList();

                    List<Interface> interfaces = await _authManagementRepository.GetInterfacesByModuleId(module.ModuleId);

                    foreach (Interface iface in interfaces)
                    {
                        InterfaceDto interfaceDto = new InterfaceDto
                        {
                            InterfaceId = iface.InterfaceId,
                            Name = iface.Name,
                            Code = iface.Code,
                            HasCreate = false,
                            HasRead = false,
                            HasUpdate = false,
                            HasDeactivate = false,
                            Create = false,
                            Read = false,
                            Update = false,
                            Deactivate = false
                        };

                        List<ModuleInterfacePermission> moduleInterfacePermissions = await _authManagementRepository.GetModuleInterfacePermissions(module.ModuleId, iface.InterfaceId);

                        foreach (ModuleInterfacePermission perm in moduleInterfacePermissions)
                        {
                            if (crudPermissions.Any(cp => cp.PermissionId == perm.PermissionId))
                            {
                                switch (perm.Permission.Code)
                                {
                                    case "CREATE":
                                        interfaceDto.HasCreate = perm.IsEnabled ?? false;
                                        break;
                                    case "READ":
                                        interfaceDto.HasRead = perm.IsEnabled ?? false;
                                        break;
                                    case "UPDATE":
                                        interfaceDto.HasUpdate = perm.IsEnabled ?? false;
                                        break;
                                    case "DEACTIVATE":
                                        interfaceDto.HasDeactivate = perm.IsEnabled ?? false;
                                        break;
                                }
                            }
                        }

                        moduleDto.Interfaces.Add(interfaceDto);
                    }

                    roleDto.Permissions.Add(moduleDto);
                }

                foreach (RoleModuleInterfacePermission rmip in roleModuleInterfacePermissions)
                {
                    MpsPermission permission = rmip.ModuleInterfacePermission.Permission;
                    Interface interfaceEntity = rmip.ModuleInterfacePermission.Interface;
                    ModuleDto moduleDto = roleDto.Permissions.FirstOrDefault(m => m.ModuleId == rmip.ModuleInterfacePermission.ModuleId);

                    if (moduleDto == null)
                    {
                        continue;
                    }

                    if (interfaceEntity != null)
                    {
                        InterfaceDto interfaceDto = moduleDto.Interfaces.FirstOrDefault(i => i.InterfaceId == interfaceEntity.InterfaceId);
                        if (interfaceDto == null)
                        {
                            interfaceDto = new InterfaceDto
                            {
                                InterfaceId = interfaceEntity.InterfaceId,
                                Name = interfaceEntity.Name,
                                Code = interfaceEntity.Code,
                                HasCreate = false,
                                HasRead = false,
                                HasUpdate = false,
                                HasDeactivate = false,
                                Create = false,
                                Read = false,
                                Update = false,
                                Deactivate = false
                            };
                            moduleDto.Interfaces.Add(interfaceDto);
                        }

                        bool hasPermission = rmip.ModuleInterfacePermission.IsEnabled ?? false;
                        switch (permission.Code)
                        {
                            case "CREATE":
                                interfaceDto.Create = hasPermission;
                                break;
                            case "READ":
                                interfaceDto.Read = hasPermission;
                                break;
                            case "UPDATE":
                                interfaceDto.Update = hasPermission;
                                break;
                            case "DEACTIVATE":
                                interfaceDto.Deactivate = hasPermission;
                                break;
                        }
                    }
                    else
                    {
                        PermissionDto permissionDto = moduleDto.OtherPermissions.FirstOrDefault(p => p.PermissionId == permission.PermissionId);
                        if (permissionDto == null)
                        {
                            permissionDto = new PermissionDto
                            {
                                PermissionId = permission.PermissionId,
                                Name = permission.Name,
                                Code = permission.Code,
                                Checked = rmip.IsActive
                            };
                            moduleDto.OtherPermissions.Add(permissionDto);
                        }
                        else
                        {
                            permissionDto.Checked = rmip.IsActive;
                        }
                    }
                }

                return new BaseResponse<RoleDto>
                {
                    Success = true,
                    Data = roleDto,
                    Message = "ROLE_RETRIEVED_SUCCESSFULLY"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<RoleDto>
                {
                    Success = false,
                    Data = null,
                    Message = "ERROR_RETRIEVING_ROLE"
                };
            }
        }

        public async Task<BaseResponse<RoleDto>> UpdateRoleWithPermissions(int roleId, RoleUpdateDto roleUpdateDto)
        {
            using (IDbContextTransaction transaction = await _authManagementRepository.CreateTransaction())
            {
                try
                {
                    Role existingRole = await _authManagementRepository.GetRoleById(roleId);
                    if (existingRole == null)
                    {
                        return new BaseResponse<RoleDto>
                        {
                            Success = false,
                            Data = null,
                            Message = "ROLE_NOT_FOUND"
                        };
                    }

                    existingRole.Name = roleUpdateDto.Name;
                    existingRole.UserGroupId = roleUpdateDto.UserGroupId;
                    existingRole.IsActive = roleUpdateDto.IsActive;

                    await _authManagementRepository.UpdateRole(existingRole);

                    List<RoleModuleInterfacePermission> existingPermissions = await _authManagementRepository.GetRoleModuleInterfacePermissions(roleId);
                    List<RoleModuleInterfacePermission> updatedPermissions = new List<RoleModuleInterfacePermission>();

                    foreach (ModuleDto module in roleUpdateDto.Permissions)
                    {
                        foreach (PermissionDto otherPermission in module.OtherPermissions)
                        {
                            ModuleInterfacePermission moduleInterfacePermission = await _authManagementRepository.GetModuleInterfacePermissionByPermissionId(otherPermission.PermissionId);
                            if (moduleInterfacePermission != null)
                            {
                                RoleModuleInterfacePermission existingPermission = existingPermissions.FirstOrDefault(ep => ep.ModuleInterfacePermissionId == moduleInterfacePermission.ModuleInterfacePermissionId);

                                if (otherPermission.Checked)
                                {
                                    if (existingPermission == null)
                                    {
                                        RoleModuleInterfacePermission newPermission = new RoleModuleInterfacePermission
                                        {
                                            RoleId = existingRole.RoleId,
                                            ModuleInterfacePermissionId = moduleInterfacePermission.ModuleInterfacePermissionId,
                                            CreatedBy = 189  // Replace with actual user ID
                                        };
                                        await _authManagementRepository.CreateRoleModuleInterfacePermission(newPermission);
                                        updatedPermissions.Add(newPermission);
                                    }
                                }
                                else
                                {
                                    if (existingPermission != null)
                                    {
                                        await _authManagementRepository.DeleteRoleModuleInterfacePermission(existingPermission);
                                    }
                                }
                            }
                        }

                        foreach (InterfaceDto iface in module.Interfaces)
                        {
                            List<(string Code, bool IsEnabled)> interfacePermissions = new List<(string Code, bool IsEnabled)>
                        {
                            ("CREATE", iface.Create),
                            ("READ", iface.Read),
                            ("UPDATE", iface.Update),
                            ("DEACTIVATE", iface.Deactivate)
                        };

                            foreach (var (code, isEnabled) in interfacePermissions)
                            {
                                ModuleInterfacePermission moduleInterfacePermission = await _authManagementRepository.GetModuleInterfacePermissionByInterfaceIdAndPermissionCode(module.ModuleId, iface.InterfaceId, code);
                                if (moduleInterfacePermission != null)
                                {
                                    RoleModuleInterfacePermission existingPermission = existingPermissions.FirstOrDefault(ep => ep.ModuleInterfacePermissionId == moduleInterfacePermission.ModuleInterfacePermissionId);

                                    if (isEnabled)
                                    {
                                        if (existingPermission == null)
                                        {
                                            RoleModuleInterfacePermission newPermission = new RoleModuleInterfacePermission
                                            {
                                                RoleId = existingRole.RoleId,
                                                ModuleInterfacePermissionId = moduleInterfacePermission.ModuleInterfacePermissionId,
                                                CreatedBy = 189  // Replace with actual user ID
                                            };
                                            await _authManagementRepository.CreateRoleModuleInterfacePermission(newPermission);
                                            updatedPermissions.Add(newPermission);
                                        }
                                    }
                                    else
                                    {
                                        if (existingPermission != null)
                                        {
                                            await _authManagementRepository.DeleteRoleModuleInterfacePermission(existingPermission);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    RoleDto roleResponseDto = _mapper.Map<RoleDto>(existingRole);
                    return new BaseResponse<RoleDto>
                    {
                        Success = true,
                        Data = roleResponseDto,
                        Message = "ROLE_UPDATED_SUCCESSFULLY"
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error updating role permissions: {ex.Message}");
                    return new BaseResponse<RoleDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "ERROR_UPDATING_ROLE"
                    };
                }
            }
        }

        public async Task<BaseResponse<Result<BaseRoleDto>>> GetAllRoleDetails(RolePaginatedRequest request)
        {
            IQueryable<Role> roles = _authManagementRepository.GetAllRoles(request);
            List<BaseRoleDto> roleDtos = await roles.Select(x => _mapper.Map<BaseRoleDto>(x)).ToListAsync();

            long rowCount = _authManagementRepository.GetRolesRowCount(request);

            BaseResponse<Result<BaseRoleDto>> response = new BaseResponse<Result<BaseRoleDto>>
            {
                Success = true,
                Data = new Result<BaseRoleDto>
                {
                    Entities = roleDtos.ToArray(),
                    Pagination = new Pagination
                    {
                        Length = rowCount,
                        PageSize = request.PagingAndSortingInfo.Paging.PageSize
                    }
                },
                Message = rowCount > 0 ? "ROLES_RETRIEVED_SUCCESSFULLY" : "NO_ROLES_FOUND"
            };

            return response;
        }
    }
}
