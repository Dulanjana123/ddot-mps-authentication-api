using Model.Response;

namespace Model.Dtos
{
    public class RoleDetailsWithPermissionsDto
    {
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public string? Name { get; set; }
        public int? SortId { get; set; }
        public bool? IsActive { get; set; }
        public int[]? Permissions { get; set; }
        public int? UserGroupId { get; set; }
    }

    public class RoleModuleInterfacePermissionDto
    {
        public int ModuleInterfacePermissionId { get; set; }
        public int RoleId { get; set; }
    }
}
