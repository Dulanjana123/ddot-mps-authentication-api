using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using AutoMapper;
using Core.CoreSettings;
using Core.Utilities;
using DataAccess.Entities;
using DataAccess.Repositories;
using DDOT.MPS.Auth.Api.Managers;
using Microsoft.EntityFrameworkCore.Storage;
using Model.Dtos;
using Model.Response;
using static Core.Exceptions.UserDefinedException;
using Model.Request.Generic;
using Test.Helpers;

namespace DDOT.MPS.Auth.Api.Tests
{
    [TestFixture]
    public class PermissionsManagerTests
    {
        private Mock<IMpsAuthManagementRepository> _authManagementRepositoryMock;
        private Mock<IMpsUserRepository> _userRepositoryMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IAppUtils> _appUtilsMock;
        private Mock<IOptions<GlobalAppSettings>> _globalAppSettingsMock;
        private PermissionsManager _permissionsManager;
        private Mock<IPermissionRepository> _permissionsRepositoryMock;
        [SetUp]
        public void SetUp()
        {
            _authManagementRepositoryMock = new Mock<IMpsAuthManagementRepository>();
            _userRepositoryMock = new Mock<IMpsUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _appUtilsMock = new Mock<IAppUtils>();
            _globalAppSettingsMock = new Mock<IOptions<GlobalAppSettings>>();
            _globalAppSettingsMock.Setup(gs => gs.Value).Returns(new GlobalAppSettings());
            _permissionsRepositoryMock = new Mock<IPermissionRepository>();
            _permissionsManager = new PermissionsManager(
                _authManagementRepositoryMock.Object,
                _userRepositoryMock.Object,
                _mapperMock.Object,
                _globalAppSettingsMock.Object,
                _appUtilsMock.Object, _permissionsRepositoryMock.Object);
        }

        [Test]
        public async Task GetAllUserGroups_ShouldReturnUserGroups_WhenUserGroupsExist()
        {
            // Arrange
            List<UserGroup> userGroups = new List<UserGroup>
            {
                new UserGroup { UserGroupId = 1, Code = "ADMIN", Name = "Admin", IsActive = true }
            };
            List<UserGroupDto> userGroupDtos = new List<UserGroupDto>
            {
                new UserGroupDto { UserGroupId = 1, Code = "ADMIN", Name = "Admin" }
            };

            _authManagementRepositoryMock.Setup(repo => repo.GetAllActiveUserGroups()).ReturnsAsync(userGroups);
            _mapperMock.Setup(m => m.Map<List<UserGroupDto>>(userGroups)).Returns(userGroupDtos);

            // Act
            BaseResponse<List<UserGroupDto>> result = await _permissionsManager.GetAllUserGroups();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("USER_GROUPS_RETRIEVED_SUCCESSFULLY", result.Message);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, result.Data.Count);
            Assert.AreEqual("ADMIN", result.Data[0].Code);
        }

        [Test]
        public async Task GetAllUserGroups_ShouldReturnNoUserGroups_WhenNoUserGroupsExist()
        {
            // Arrange
            _authManagementRepositoryMock.Setup(repo => repo.GetAllActiveUserGroups()).ReturnsAsync(new List<UserGroup>());

            // Act
            BaseResponse<List<UserGroupDto>> result = await _permissionsManager.GetAllUserGroups();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("NO_USER_GROUPS_FOUND", result.Message);
            Assert.IsNull(result.Data);
        }

        [Test]
        public async Task GetAllUserGroups_ShouldNotIncludeInactiveUserGroups()
        {
            // Arrange
            List<UserGroup> userGroups = new List<UserGroup>
            {
                new UserGroup { UserGroupId = 1, Code = "ADMIN", Name = "Admin", IsActive = true },
                new UserGroup { UserGroupId = 2, Code = "ABC", Name = "ABC", IsActive = false }
            };

            List<UserGroupDto> userGroupDtos = new List<UserGroupDto>
            {
                new UserGroupDto { UserGroupId = 1, Code = "ADMIN", Name = "Admin" }
            };

            _authManagementRepositoryMock.Setup(repo => repo.GetAllActiveUserGroups()).ReturnsAsync(userGroups.Where(ug => ug.IsActive == true).ToList());
            _mapperMock.Setup(m => m.Map<List<UserGroupDto>>(It.IsAny<List<UserGroup>>())).Returns(userGroupDtos);

            // Act
            BaseResponse<List<UserGroupDto>> result = await _permissionsManager.GetAllUserGroups();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("USER_GROUPS_RETRIEVED_SUCCESSFULLY", result.Message);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, result.Data.Count);
            Assert.AreEqual("ADMIN", result.Data[0].Code);
        }

        [Test]
        public async Task CreateModuleInterfacePermission_ShouldReturnError_WhenModuleDoesNotExist()
        {
            // Arrange
            ModuleInterfacePermissionDto permissionDto = new ModuleInterfacePermissionDto { ModuleId = 1, PermissionId = 1 };
            _authManagementRepositoryMock.Setup(repo => repo.GetModuleById(permissionDto.ModuleId)).ReturnsAsync((Module)null);

            // Act
            BaseResponse<ModuleInterfacePermissionResponseDto> result = await _permissionsManager.CreateModuleInterfacePermission(permissionDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("INVALID_MODULE_ID");
            result.Data.Should().BeNull();
        }

        [Test]
        public async Task CreateModuleInterfacePermission_ShouldReturnError_WhenInterfaceDoesNotExist()
        {
            // Arrange
            ModuleInterfacePermissionDto permissionDto = new ModuleInterfacePermissionDto { ModuleId = 1, InterfaceId = 1, PermissionId = 1 };
            _authManagementRepositoryMock.Setup(repo => repo.GetModuleById(permissionDto.ModuleId)).ReturnsAsync(new Module());
            _authManagementRepositoryMock.Setup(repo => repo.GetInterfaceById(permissionDto.InterfaceId.Value)).ReturnsAsync((Interface)null);

            // Act
            BaseResponse<ModuleInterfacePermissionResponseDto> result = await _permissionsManager.CreateModuleInterfacePermission(permissionDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("INVALID_INTERFACE_ID");
            result.Data.Should().BeNull();
        }

        [Test]
        public async Task CreateModuleInterfacePermission_ShouldReturnError_WhenPermissionDoesNotExist()
        {
            // Arrange
            ModuleInterfacePermissionDto permissionDto = new ModuleInterfacePermissionDto { ModuleId = 1, PermissionId = 1 };
            _authManagementRepositoryMock.Setup(repo => repo.GetModuleById(permissionDto.ModuleId)).ReturnsAsync(new Module());
            _authManagementRepositoryMock.Setup(repo => repo.GetPermissionById(permissionDto.PermissionId)).ReturnsAsync((MpsPermission)null);

            // Act
            BaseResponse<ModuleInterfacePermissionResponseDto> result = await _permissionsManager.CreateModuleInterfacePermission(permissionDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("INVALID_PERMISSION_ID");
            result.Data.Should().BeNull();
        }

        [Test]
        public async Task CreateModuleInterfacePermission_ShouldReturnError_WhenPermissionAlreadyExists()
        {
            // Arrange
            ModuleInterfacePermissionDto permissionDto = new ModuleInterfacePermissionDto { ModuleId = 1, PermissionId = 1 };
            _authManagementRepositoryMock.Setup(repo => repo.GetModuleById(permissionDto.ModuleId)).ReturnsAsync(new Module());
            _authManagementRepositoryMock.Setup(repo => repo.GetPermissionById(permissionDto.PermissionId)).ReturnsAsync(new MpsPermission());
            _authManagementRepositoryMock.Setup(repo => repo.ModuleInterfacePermissionExists(permissionDto.ModuleId, null, permissionDto.PermissionId)).ReturnsAsync(true);

            // Act
            BaseResponse<ModuleInterfacePermissionResponseDto> result = await _permissionsManager.CreateModuleInterfacePermission(permissionDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("MODULE_PERMISSION_ALREADY_EXISTS");
            result.Data.Should().BeNull();
        }

        [Test]
        public async Task CreateModuleInterfacePermission_ShouldCreatePermissionSuccessfully()
        {
            // Arrange
            var permissionDto = new ModuleInterfacePermissionDto { ModuleId = 1, PermissionId = 1 };
            var module = new Module { ModuleId = 1, Code = "MOD1" };
            var permission = new MpsPermission { PermissionId = 1, Code = "PERM1" };
            var createdPermission = new ModuleInterfacePermission
            {
                ModuleId = 1,
                PermissionId = 1,
                Code = "MOD1_PERM1"
            };

            var createdPermissionResponse = new ModuleInterfacePermissionResponseDto
            {
                Code = "MOD1_PERM1"
            };

            _authManagementRepositoryMock.Setup(repo => repo.GetModuleById(permissionDto.ModuleId)).ReturnsAsync(module);
            _authManagementRepositoryMock.Setup(repo => repo.GetPermissionById(permissionDto.PermissionId)).ReturnsAsync(permission);
            _authManagementRepositoryMock.Setup(repo => repo.ModuleInterfacePermissionExists(permissionDto.ModuleId, null, permissionDto.PermissionId)).ReturnsAsync(false);
            _mapperMock.Setup(mapper => mapper.Map<ModuleInterfacePermission>(permissionDto)).Returns(createdPermission);
            _authManagementRepositoryMock.Setup(repo => repo.CreateModuleInterfacePermissionAsync(createdPermission)).ReturnsAsync(createdPermission);

            _mapperMock.Setup(mapper => mapper.Map<ModuleInterfacePermissionResponseDto>(createdPermission)).Returns(createdPermissionResponse); // UPDATE: Ensure mapping is set up correctly

            // Act
            var result = await _permissionsManager.CreateModuleInterfacePermission(permissionDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("MODULE_INTERFACE_PERMISSION_CREATED_SUCCESSFULLY");
            result.Data.Should().NotBeNull(); // Ensure that Data is not null
            result.Data.Code.Should().Be("MOD1_PERM1");
        }

        [Test]
        public async Task GetNextRoleCode_ShouldReturnNextRoleCode_WhenLatestRoleCodeExists()
        {
            // Arrange
            _authManagementRepositoryMock.Setup(repo => repo.GetLatestRoleCode()).ReturnsAsync("005");

            // Act
            BaseResponse<string> result = await _permissionsManager.GetNextRoleCode();

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("NEXT_ROLE_CODE_GENERATED_SUCCESSFULLY");
            result.Data.Should().Be("006");
        }

        [Test]
        public async Task GetNextRoleCode_ShouldReturnError_WhenLatestRoleCodeIsInvalid()
        {
            // Arrange
            _authManagementRepositoryMock.Setup(repo => repo.GetLatestRoleCode()).ReturnsAsync("InvalidCode");

            // Act
            BaseResponse<string> result = await _permissionsManager.GetNextRoleCode();

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("ERROR_PARSING_ROLE_CODE");
            result.Data.Should().BeNull();
        }

        [Test]
        public async Task GetNextRoleCode_ShouldReturnFirstRoleCode_WhenNoPreviousCodeExists()
        {
            // Arrange
            _authManagementRepositoryMock.Setup(repo => repo.GetLatestRoleCode()).ReturnsAsync((string)null);

            // Act
            BaseResponse<string> result = await _permissionsManager.GetNextRoleCode();

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("NEXT_ROLE_CODE_GENERATED_SUCCESSFULLY");
            result.Data.Should().Be("001");
        }

        [Test]
        public async Task GetRoleWithPermissions_ShouldReturnError_WhenRoleNotFound()
        {
            // Arrange
            int roleId = 1;
            _authManagementRepositoryMock.Setup(repo => repo.GetRoleById(roleId)).ReturnsAsync((Role)null);

            // Act
            BaseResponse<RoleDto> result = await _permissionsManager.GetRoleWithPermissions(roleId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("ROLE_NOT_FOUND");
            result.Data.Should().BeNull();
        }

        [Test]
        public async Task UpdateRoleWithPermissions_ShouldReturnError_WhenRoleNotFound()
        {
            // Arrange
            int roleId = 1;
            RoleUpdateDto roleUpdateDto = new RoleUpdateDto();
            _authManagementRepositoryMock.Setup(repo => repo.GetRoleById(roleId)).ReturnsAsync((Role)null);

            // Act
            BaseResponse<RoleDto> result = await _permissionsManager.UpdateRoleWithPermissions(roleId, roleUpdateDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("ROLE_NOT_FOUND");
            result.Data.Should().BeNull();
        }

        [Test]
        public async Task UpdateRoleWithPermissions_ShouldUpdateRoleSuccessfully()
        {
            // Arrange
            int roleId = 1;
            RoleUpdateDto roleUpdateDto = new RoleUpdateDto
            {
                Name = "UpdatedRole",
                Permissions = new List<ModuleDto>()
            };

            Role existingRole = new Role
            {
                RoleId = roleId,
                Code = "ROLE1",
                Name = "Role1",
                UserGroupId = 1,
                IsActive = false
            };

            _authManagementRepositoryMock.Setup(repo => repo.GetRoleById(roleId)).ReturnsAsync(existingRole);
            _authManagementRepositoryMock.Setup(repo => repo.CreateTransaction()).ReturnsAsync(Mock.Of<IDbContextTransaction>());
            _authManagementRepositoryMock.Setup(repo => repo.UpdateRole(It.IsAny<Role>())).Returns(Task.CompletedTask);
            _authManagementRepositoryMock.Setup(repo => repo.GetRoleModuleInterfacePermissions(roleId)).ReturnsAsync(new List<RoleModuleInterfacePermission>());

            _mapperMock.Setup(mapper => mapper.Map<RoleDto>(It.IsAny<Role>())).Returns((Role role) => new RoleDto
            {
                Code = role.Code,
                Name = role.Name,
                UserGroupId = role.UserGroupId ?? 0,
                Permissions = new List<ModuleDto>()
            });

            // Act
            BaseResponse<RoleDto> result = await _permissionsManager.UpdateRoleWithPermissions(roleId, roleUpdateDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("ROLE_UPDATED_SUCCESSFULLY");
            result.Data.Should().NotBeNull();
            result.Data.Name.Should().Be("UpdatedRole");
        }
        
        [Test]
        public async Task GetAllRoleDetails_Success()
        {
            RolePaginatedRequest request = new RolePaginatedRequest
            {
                PagingAndSortingInfo = new PagingAndSortingInfo
                {
                    Paging = new PagingInfo
                    {
                        PageNo = 1,
                        PageSize = 10
                    }
                }
            };

            IQueryable<Role> roles = new List<Role> { new Role(), new Role() }.AsQueryable();
            List<BaseRoleDto> roleDtos = new List<BaseRoleDto> { new BaseRoleDto(), new BaseRoleDto() };

            TestAsyncEnumerable<Role> testAsyncEnumerable = new TestAsyncEnumerable<Role>(roles);

            _authManagementRepositoryMock.Setup(repo => repo.GetAllRoles(It.IsAny<RolePaginatedRequest>())).Returns(testAsyncEnumerable);
            _authManagementRepositoryMock.Setup(repo => repo.GetRolesRowCount(It.IsAny<RolePaginatedRequest>())).Returns(roles.Count());

            _mapperMock.Setup(mapper => mapper.Map<BaseRoleDto>(It.IsAny<Role>())).Returns(new BaseRoleDto());
        
            BaseResponse<Result<BaseRoleDto>> result = await _permissionsManager.GetAllRoleDetails(request);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("ROLES_RETRIEVED_SUCCESSFULLY", result.Message);
            Assert.AreEqual(roles.Count(), result.Data.Entities.Length);
        }

        public async Task GetAllRoleDetails_ReturnsEmptyResponse()
        {
            RolePaginatedRequest request = new RolePaginatedRequest
            {
                PagingAndSortingInfo = new PagingAndSortingInfo
                {
                    Paging = new PagingInfo
                    {
                        PageNo = 1,
                        PageSize = 10
                    }
                }
            };

            IQueryable<Role> roles = new List<Role>().AsQueryable();
            TestAsyncEnumerable<Role> testAsyncEnumerable = new TestAsyncEnumerable<Role>(roles);

            _authManagementRepositoryMock.Setup(repo => repo.GetAllRoles(It.IsAny<RolePaginatedRequest>())).Returns(testAsyncEnumerable);
            _authManagementRepositoryMock.Setup(repo => repo.GetRolesRowCount(It.IsAny<RolePaginatedRequest>())).Returns(roles.Count());

            BaseResponse<Result<BaseRoleDto>> result = await _permissionsManager.GetAllRoleDetails(request);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("NO_ROLES_FOUND", result.Message);
            Assert.IsNull(result.Data);
        }

    }
}
