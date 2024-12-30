namespace Model.Dtos
{

    public class BaseRoleDto
    {
        public int RoleId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int? SortId { get; set; }
        public int UserGroupId { get; set; }
        public bool IsActive { get; set; }
    }

    public class RoleDto : BaseRoleDto
    {
        public List<ModuleDto> Permissions { get; set; } = new List<ModuleDto>();
    }

    public class RoleUpdateDto: RoleDto
    {
        public int RoleId { get; set; }
        public bool IsActive { get; set; }

    }
}
