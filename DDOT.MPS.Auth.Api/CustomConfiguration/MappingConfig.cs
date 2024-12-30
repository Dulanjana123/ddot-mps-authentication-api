using AutoMapper;
using Core.Enums;
using DataAccess.Entities;
using Microsoft.Graph.Models;
using Model.Dtos;
using Model.Response;

namespace DDOT.MPS.Auth.Api.CustomConfiguration
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<UserDto, MpsUser>();
                config.CreateMap<MpsUser, UserDto>();
                config.CreateMap<MpsUser, UserResponseDto>();
                config.CreateMap<UserDto, UserRegistrationB2cDto>();
                config.CreateMap<User, UserRegistrationB2cDto>();
                config.CreateMap<UserDto, UserResponseDto>();
                // Role
                config.CreateMap<RoleDto, Role>();
                config.CreateMap<Role, RoleDto>();
                config.CreateMap<RoleResponseDto, Role>();
                config.CreateMap<Role, RoleResponseDto>();
                config.CreateMap<RoleWithPermissionsDto, Role>();
                config.CreateMap<Role, RoleWithPermissionsDto>();
                config.CreateMap<CreateRoleWithPermissionsDto, Role>();
                config.CreateMap<Role, CreateRoleWithPermissionsDto>();
                config.CreateMap<RoleDetailsWithPermissionsDto, Role>(); 
                // Permission
                config.CreateMap<PermissionDto, MpsPermission>();
                config.CreateMap<MpsPermission, PermissionDto>();
                config.CreateMap<PermissionResponseDto, MpsPermission>();
                config.CreateMap<MpsPermission, PermissionResponseDto>();
                config.CreateMap<PermissionCreationDto, MpsPermission>();
                config.CreateMap<MpsPermission, PermissionCreationDto>();
                // NEW
                config.CreateMap<RoleModuleInterfacePermissionDto,RoleModuleInterfacePermission>(); 
                config.CreateMap<ModuleInterfacePermission, ModuleInterfacePermissionDto>();
                config.CreateMap<ModuleInterfacePermissionDto, ModuleInterfacePermission>();
                config.CreateMap<Role, RoleDto>()
                    .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => new List<ModuleDto>()));
                config.CreateMap<Module, ModuleDto>()
                    .ForMember(dest => dest.OtherPermissions, opt => opt.MapFrom(src => new List<PermissionDto>()))
                    .ForMember(dest => dest.Interfaces, opt => opt.MapFrom(src => new List<InterfaceDto>()));
                config.CreateMap<MpsPermission, PermissionDto>();
                config.CreateMap<Interface, InterfaceDto>();
                config.CreateMap<ModuleInterfacePermission, ModuleInterfacePermissionResponseDto>();
                config.CreateMap<ModuleInterfacePermissionResponseDto, ModuleInterfacePermission>();

                config.CreateMap<UserRole, UserRoleWithPermissionsDto>()
                        .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.Role.RoleId))
                        .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Role.Code))
                        .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Role.Name))
                        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Role.Description))
                        .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Role.RoleModuleInterfacePermissions.Select(p => p.ModuleInterfacePermission.Permission)));

                // Map from Permission to UserPermissionResponseDto
                config.CreateMap<MpsPermission, UserPermissionResponseDto>()
                    .ForMember(dest => dest.PermissionId, opt => opt.MapFrom(src => src.PermissionId))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

                // Agency
                config.CreateMap<AgencyDto, Agency>();
                config.CreateMap<Agency, AgencyDto>();
                config.CreateMap<Agency, AgencyResponseDto>();
                config.CreateMap<AgencyDto, AgencyResponseDto>();

                // DatagridState
                config.CreateMap<DatagridStateDto, SettingsGridState>();
                config.CreateMap<SettingsGridState, DatagridStateDto>();
                config.CreateMap<SettingsGridState, DatagridStateResponseDto>();
                config.CreateMap<DatagridStateResponseDto, SettingsGridState>();


                config.CreateMap<UserDetailsDto, MpsUser>();
                config.CreateMap<MpsUser, UserDetailsDto>();


                config.CreateMap<FullLoginHistoryDto, LoginHistory>()
                    .ForMember(dest => dest.UserIntractionId, opt => opt.MapFrom(src => (int)src.Userintractionid));

                config.CreateMap<LoginHistory, FullLoginHistoryDto>()
                    .ForMember(dest => dest.Userintractionid, opt => opt.MapFrom(src => (UserIntractionType)src.UserIntractionId));

                config.CreateMap<Agency, AgencyComboDto>()
                    .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.AgencyId))
                    .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.AgencyName));

                config.CreateMap<DataAccess.Entities.UserType, UserTypeComboDto>()
                    .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.UserTypeId))
                    .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.UserTypeName));

                config.CreateMap<BaseRoleDto, Role>()
                    .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                    .ForMember(dest => dest.SortId, opt => opt.MapFrom(src => src.SortId))
                    .ForMember(dest => dest.UserGroupId, opt => opt.MapFrom(src => src.UserGroupId))
                    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                    .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId)).ReverseMap();
            });

            return mappingConfig;
        }
    }
}
