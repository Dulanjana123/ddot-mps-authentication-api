namespace Model.Response
{
    public  class UserRoleWithPermissionsDto
    {
        public int RoleId { get; set; }
        public string Code { get; set; } = null!;
        public string? Name { get; set; } = null!;
        public string? Description { get; set; }
        public List<UserPermissionResponseDto> Permissions { get; set; } = new List<UserPermissionResponseDto>();
    }

    public class UserRolesWithPermissionsDto
    {
        public List<UserRoleWithPermissionsDto> Roles { get; set; } = new List<UserRoleWithPermissionsDto>();
    }
}
